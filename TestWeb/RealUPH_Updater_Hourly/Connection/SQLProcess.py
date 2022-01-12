import pyodbc
import pandas 
from Connection import ConnectionString
import datetime
from string import Template



class SqlProcess(object):
    def __init__(self,Org):
        self.conndb = ConnectionString.Connection()
        if Org == 'T1':
            connOrg = self.conndb.T1_MGU_Conn
        if Org == 'T2':
            connOrg = self.conndb.T2_MGU_Conn
        if Org == 'T3':
            connOrg = self.conndb.T3_MGU_Conn
        if Org == 'T5':
            connOrg = self.conndb.T5_MGU_Conn
        self.cnxnOrg = pyodbc.connect(connOrg)
        connPE = self.conndb.PE_Support_Conn
        self.cnxnPE = pyodbc.connect(connPE)
        self.Org = Org
 
        

    def GetItemNameTypeByHour(self,org,table,_thistime,hour):
        try:
            # thistime = datetime.datetime.strptime(thistime,"%Y-%m-%d %H:%M:%S") + datetime.timedelta(hours=-8)
            thisstrtime = _thistime.strftime('%Y-%m-%d')
            thishour = _thistime.hour
            lasttime = _thistime + datetime.timedelta(hours=hour)
            lasthour = lasttime.hour
            laststrtime = lasttime.strftime('%Y-%m-%d')
            script = "select final.ItemNameType  from (select ItemNameType, SerialNumber, Result, ROW_NUMBER() over(partition by SerialNumber order by Result) as raw_index  from ate_db.dbo.%s  where   Station like \'%s%%\'  and  tdatetime between \'%s %d:00:00\' and \'%s %d:00:00\')   final join ate_result.dbo.ItemName as itemName on final.ItemNameType = itemName.ItemNameType   where final.raw_index = 1  group by  final.ItemNameType  order by  final.ItemNameType"% (table,org,thisstrtime,thishour,laststrtime,lasthour)
            result = pandas.read_sql(script,self.cnxnOrg)
            result['table'] = table
            return result
        except:
            return None

    def GetNoITMXPDataByHour(self,org,_thistime,hour,Data_type):
        try:
            script = self.GetNoITMXPScript(_thistime,hour,Data_type)
            result = pandas.read_sql(script,self.cnxnOrg)
            result['table'] = "tblfinal"
            return result
        except:
            return None

    def TruncateUPHTable(self):
        try:
            script = 'truncate table [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput]'
            cursor = self.cnxnPE.cursor()
            cursor.execute(script) 
            self.cnxnPE.commit()
            return True
        except:
            return False

    def GetDescriptionByItemNameType(self,itemnametype):
        try:
            itemlist = '(0'
            
            for item in itemnametype:
                itemlist = itemlist + ',' +str(item)
            itemlist = itemlist+')'

            script ='SELECT ItemNameType,ItemDescription as productname  FROM [ate_result].[dbo].[ItemName] where ItemNameType in %s'%itemlist
            result = pandas.read_sql(script,self.cnxnOrg)
            return result            
        except:
            return None

    def GetItemNameType_NonITMPXP(self,ItemDescList):
        try:
            itemlist = '('
            
            for item in ItemDescList:
                itemlist = itemlist +'\''+str(item)+'\'' +','
            itemlist = itemlist+'\'0\')'

            script ='SELECT  [ItemNameType],[Product_Des] as productname FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ItemNameType]  where Product_Des in  %s'%itemlist
            result = pandas.read_sql(script,self.cnxnPE)
            return result            
        except:
            return None


    def GetTestTypeByItemNameType(self,itemnametype):
        try:
            itemlist = '(0'
            for item in itemnametype:
                itemlist = itemlist + ',' +str(item)
            itemlist = itemlist+')'
            script ='select a.ItemNameType,a.TestType,a.TestType2 from (SELECT ItemNameType,TestType,TestType2 ,Date, ROW_NUMBER() over(partition by ItemNameType order by date desc) as raw_index FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where ItemNameType in %s group by itemnametype,TestType,TestType2,Date) a where a.raw_index = 1'%itemlist
            result = pandas.read_sql(script,self.cnxnPE)
            return result            
        except:
            return None


    def GetGPNByItemNameType(self,table,_thistime,itemnametype):
        try:
            itemlist = '(0'
            for item in itemnametype:
                itemlist = itemlist + ',' +str(item)
            itemlist = itemlist+')'
            thisstrtime = _thistime.strftime('%Y-%m-%d')
            thishour = _thistime.hour
            lasttime = _thistime + datetime.timedelta(hours=1)
            lasthour = lasttime.hour
            laststrtime = lasttime.strftime('%Y-%m-%d')
            
            if table == 'tblcpu':
                script = "select  cpu.ItemNameType,cpu.gpn from (select ItemNameType, (SUBSTRING(FixtureID1,1,3)+'-'+SUBSTRING(FixtureID1,4,5)+'-'+SUBSTRING(FixtureID1,9,2)) as gpn ,ROW_NUMBER() over(partition by ItemNameType order by ItemNameType) as raw_index  from tblcpu where ItemNameType in %s and tDateTime between \'%s %d:00:00\' and \'%s %d:00:00\'  ) cpu join ate_result.dbo.ItemName as itemName on cpu.ItemNameType = itemName.ItemNameType   where cpu.raw_index = 1  "% (itemlist,thisstrtime,thishour,laststrtime,lasthour)
            else:
                script = "Select  final.ItemNameType,final.gpn FROM (select ItemNameType, NOHGPN as gpn,ROW_NUMBER() over(partition by ItemNameType order by ItemNameType) as raw_index   from ate_db.dbo.tblfinal where ItemNameType in %s and tDateTime between \'%s %d:00:00\' and \'%s %d:00:00\' )  final join ate_result.dbo.ItemName as itemName on final.ItemNameType = itemName.ItemNameType   where final.raw_index = 1  "%(itemlist,thisstrtime,thishour,laststrtime,lasthour)
            result = pandas.read_sql(script,self.cnxnOrg)
            return result            
        except:
            return 'xxx-xxxxx-xx'    

    def GetGPNByProductName(self,ItemList):
        Output = pandas.DataFrame([], columns=['productname','gpn'])
        for index,row in ItemList.iterrows():
            TempNickName = row['productname'].split('_')[0]
            script = 'Select top(1) NOHGPN from tblfinal where ExeInfo like \'%%%s%%\'and NOHGPN!=\'\' and NOHGPN!= \'XXX-YYYYY-ZZ\'  and SubString(NOHGPN,1,3)=\'010\' order by tDateTime desc'%(TempNickName)    

            result = pandas.read_sql(script,self.cnxnOrg)
            if result.empty != True:
                gpn = result['NOHGPN'][0]
                new=pandas.DataFrame({'productname':row['productname'],'gpn':gpn[:-2]+'XX'},index=[1])
                Output = Output.append(new)       
            if result.empty == True:
                new=pandas.DataFrame({'productname':row['productname'],'gpn':'XXX-XXXXX-XX'},index=[1])
                Output = Output.append(new)
        return Output


    
    def GetUPHFromProductionMap(self,_input):
        try:
            # Output = pandas.DataFrame(data = [['0',0]],columns = ['ItemNameType','uph'])
            # Output = _input['ItemNameType'].to_frame().copy()
            # print(Output)
            # Output.insert(1,'uph',999)
            Output = pandas.DataFrame([], columns=['ItemNameType','uph'])
            for index,row in _input.iterrows():
                try:
                    thisitem = int(row['ItemNameType'])
                    if 'xx' in row['gpn'] and row['gpn']!='XXX-XXXXX-XX':
                        gpnstring = ' like \''+ row['gpn'].replace('XX','%') +'\''
                    else:
                        gpnstring = ' = \''+ row['gpn']+'\''

                    if row['TestType2'] =='':
                        row['TestType2']='NA'
                    if row['table']=='tblcpu':
                        script = "SELECT top(1)POH  FROM [CAPA_DB].[dbo].[CAPA_Process_Map_SMT]  WHERE Top_Item %s and TestType=\'%s\'  order by Refresh_time desc"%(gpnstring,row['TestType'])                   
                    else:
                        script = "SELECT top(1)POH  FROM [CAPA_DB].[dbo].[CAPA_Process_Map]  WHERE Top_Item %s and TestType=\'%s\' and TestType2 = \'%s\'  order by Refresh_time desc"%(gpnstring,row['TestType'],row['TestType2'])                   
                        
                    result = pandas.read_sql(script,self.cnxnPE)
                    if result is not None:
                        uph = int(result['POH'][0])
                        new=pandas.DataFrame({'ItemNameType':int(thisitem),'uph':uph},index=[1])
                        Output = Output.append(new)       
                    if result is None:
                        new=pandas.DataFrame({'ItemNameType':int(thisitem),'uph':999},index=[1])
                        Output = Output.append(new)      
                except:
                    new=pandas.DataFrame({'ItemNameType':int(thisitem),'uph':999},index=[1])
                    Output = Output.append(new)
         
            Output['ItemNameType'] = Output['ItemNameType'].astype('int')
            return Output         
        except:
            Output['ItemNameType'] = Output['ItemNameType'].astype('int')
            return Output

    def GetUPHByProductNamw_NonITMXP(self,ItemNameList):
        Output = pandas.DataFrame([], columns=['productname','uph'])
        for index,row in ItemNameList.iterrows():
            if row['gpn'] == 'XXX-XXXXX-XX':
                new=pandas.DataFrame({'productname':row['productname'],'uph':str(999)},index=[1])
                Output = Output.append(new)
            else:
                gpnstring = ' like \''+ row['gpn'][:-2] +'%%\''
                script = "SELECT top(1)POH  FROM [CAPA_DB].[dbo].[CAPA_Process_Map]  WHERE Top_Item %s and TestType=\'%s\' and TestType2 = \'%s\'  order by Refresh_time desc"%(gpnstring,row['TestType'],row['TestType2'])                   

                result = pandas.read_sql(script,self.cnxnPE)
                if result.empty != True:
                    uph = int(result['POH'][0])
                    new=pandas.DataFrame({'productname':row['productname'],'uph':uph},index=[1])
                    Output = Output.append(new)       
                if result.empty == True:
                    new=pandas.DataFrame({'productname':row['productname'],'uph':str(999)},index=[1])
                    Output = Output.append(new)
        return Output

    def GetRealOutputByItemNameType(self,table,_thistime,itemnametype):
        try:
            itemlist = '(0'
            for item in itemnametype:
                itemlist = itemlist + ',' +str(item)
            itemlist = itemlist+')'
            thisstrtime = _thistime.strftime('%Y-%m-%d')
            thishour = _thistime.hour
            lasttime = _thistime + datetime.timedelta(hours=1)
            lasthour = lasttime.hour
            laststrtime = lasttime.strftime('%Y-%m-%d')

            script = "select ItemNameType,COUNT(*)as RealOutput,(cast(NULLIF(Datediff(MINUTE,MIN(tDateTime),MAX(tDateTime)),0)as float)/60)as EstimateHours,COUNT(*)/(cast(NULLIF(Datediff(MINUTE,MIN(tDateTime),MAX(tDateTime)),0)as float)/60) AS EstimateUPH ,AVG(cast(Spare as int)) as AvgSpare from  ate_db.dbo.%s where itemnametype in %s  and tDateTime BETWEEN \'%s %d:00:00\' AND \'%s %d:00:00\' AND cast(FailItem as float) =0 group by  Itemnametype"%(table,itemlist,thisstrtime,thishour,laststrtime,lasthour)
            result = pandas.read_sql(script,self.cnxnOrg)
            return result
        except Exception as e:
            print(e)
            return None
    
    # TODO
    def GetFYRByItemNameType(self,table,_thistime,itemnametype):
        try:
            itemlist = '(0'
            for item in itemnametype:
                itemlist = itemlist + ',' +str(item)
            itemlist = itemlist+')'
            thisstrtime = _thistime.strftime('%Y-%m-%d')
            thishour = _thistime.hour
            lasttime = _thistime + datetime.timedelta(hours=1)
            lasthour = lasttime.hour
            laststrtime = lasttime.strftime('%Y-%m-%d')

            script = "select ItemNameType,cast(sum(CAST(Result AS INT)) as float)/cast(count(*) as float) as FYR from (select ItemNameType,SerialNumber,tDateTime, ROW_NUMBER() over(partition by ItemNameType,serialnumber order by tdatetime) as raw_index ,Result from ate_db.dbo.%s where ItemNameType in %s and tDateTime BETWEEN \'%s %d:00:00\' AND \'%s %d:00:00\' and SerialNumber !='9999999999' group by ItemNameType,SerialNumber,tDateTime,Result) as t where t.raw_index = 1 group by ItemNameType "%(table,itemlist,thisstrtime,thishour,laststrtime,lasthour)
            result = pandas.read_sql(script,self.cnxnOrg)
            return result
        except Exception as e:
            print(e)
            return None


    def InsertDataToRealOutputTable(self,Input,shiftid):
        try:
            Input = Input.fillna(0)
            cursor = self.cnxnPE.cursor()
            script ="INSERT INTO  [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput] (Org, TimeIndex,ItemNameType,[table],productname,TestType,TestType2,gpn,uph,RealOutput,EstimateHours,EstimateUPH,AvgSpare,shiftid,Date,FYR )  VALUES "

            for index,row in Input.iterrows():
                timeindex = int(row['TimeIndex'])
                script +="(\'%s\',%d,%d,\'%s\',\'%s\',\'%s\',\'%s\',\'%s\',%s,%d,%f,%f,%f,%d,\'%s\',%f)"% (row['Org'],timeindex,row['ItemNameType'],row['table'],row['productname'],row['TestType'],row['TestType2'],row['gpn'],row['uph'],row['RealOutput'],row['EstimateHours'],row['EstimateUPH'],row['AvgSpare'],shiftid,datetime.datetime.today().strftime('%Y-%m-%d'),row['FYR'])            
                if(index!= len(Input)-1):
                    script+=','
            self.cnxnPE.execute(script)  
            cursor.commit()     
            return True
        except:
            return False





#NoITMXP bk
    # def GetUPHByProductNamw_NonITMXP(self,ItemNameList):
    #     Output = pandas.DataFrame([], columns=['productname','uph'])
    #     for index,row in ItemNameList.iterrows():
    #         TempNickName = row['productname'].split('_')[0]
    #         script = "SELECT top(1)POH  FROM [CAPA_DB].[dbo].[CAPA_Process_Map]  WHERE Description like \'%%%s%%\' and TestType=\'%s\' and TestType2 = \'%s\'  order by Refresh_time desc"%(TempNickName,row['TestType'],row['TestType2'])     
    #         # script = "SELECT top(1)POH  FROM [CAPA_DB].[dbo].[CAPA_Process_Map]  WHERE Description like \'%%Approach S60%%\' and TestType=\'%s\' and TestType2 = \'%s\'  order by Refresh_time desc"%(row['TestType'],row['TestType2'])     

    #         result = pandas.read_sql(script,self.cnxnPE)
    #         if result.empty != True:
    #             uph = int(result['POH'][0])
    #             new=pandas.DataFrame({'productname':row['productname'],'uph':uph},index=[1])
    #             Output = Output.append(new)       
    #         if result.empty == True:
    #             new=pandas.DataFrame({'productname':row['productname'],'uph':str(999)},index=[1])
    #             Output = Output.append(new)
    #     return Output
    

    def GetNoITMXPScript(self,_thistime,hour,Data_type):

            lasttime = _thistime + datetime.timedelta(hours=hour)
            thisdatetime = "\'%s %d:00:00\'"%(_thistime.strftime('%Y-%m-%d'),_thistime.hour)
            lastdatetime = "\'%s %d:00:00\'"%(lasttime.strftime('%Y-%m-%d'),lasttime.hour)       
            _script =""
            if Data_type =="Athena":
                    _script =  '''select FinalTable.ItemDescription as productname,FinalTable.TestType,FinalTable.TestType2,sum(FinalTable.Result) as RealOutput ,AVG(cast(FinalTable.Spare as int)) as AvgSpare,
                                (cast(NULLIF(Datediff(MINUTE,MIN(finaltable.tDateTime),MAX(finaltable.tDateTime)),0)as float)/60)as EstimateHours,COUNT(*)/(cast(NULLIF(Datediff(MINUTE,MIN(finaltable.tDateTime),MAX(finaltable.tDateTime)),0)as float)/60) AS EstimateUPH 
                                from  (select A.ESN,A.Result,A.TestType,A.TestType2,A.tDateTime, CASE
                                    WHEN A.TestType = \'W\' THEN REPLACE(B.ItemDescription, \'RTESN\', \'Gsensor\')
                                    WHEN A.TestType = \'R\' THEN REPLACE(B.ItemDescription, \'RTESN\', \'Compass\')   
                                    END AS ItemDescription,A.Spare FROM    
                                    (Select distinct C.SerialNumber AS ESN, C.tDateTime, C.Result, C.TestType,C.TestType2, D.ItemNameType, C.Spare FROM   
                                    (select SerialNumber, tDateTime, Result, Spare, TestType,TestType2 FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock)   
                                    where  tdatetime between ''' + thisdatetime+ " and " + lastdatetime+'''and ExeInfo like '%Athena%'  and itemnametype in (\'451\', \'453\'))C   
                                    inner join   
                                    (select ItemNameType, SerialNumber  FROM[ate_db_tblfinal_new].[dbo].[TblFinal]where TestType2 = 183)D   
                                    on C.SerialNumber = D.SerialNumber)A   
                                    inner join(select itemnametype, ItemDescription from[ate_result].[dbo].[ItemName])B  
                                    on A.ItemNameType = B.ItemNameType ) FinalTable group by ItemDescription,TestType,TestType2 order by ItemDescription desc'''
            elif Data_type == "uTube" or Data_type == "Baro":
                _script =   '''select finaltable.ItemDescription as productname,finaltable.TestType,finaltable.TestType2,sum(finaltable.Result) as RealOutput,AVG(finaltable.Spare) AS AvgSpare ,
                            (cast(NULLIF(Datediff(MINUTE,MIN(finaltable.tDateTime),MAX(finaltable.tDateTime)),0)as float)/60)as EstimateHours,COUNT(*)/(cast(NULLIF(Datediff(MINUTE,MIN(finaltable.tDateTime),MAX(finaltable.tDateTime)),0)as float)/60) AS EstimateUPH
                            from (select A.ESN,A.Result,A.TestType,A.TestType2,A.tDateTime,
                            REPLACE(B.ItemDescription, 'RTESN', \''''+ Data_type + '\')' + ''' AS ItemDescription,A.Spare FROM   
                            (Select distinct C.SerialNumber AS ESN, C.tDateTime, C.Result, C.TestType,C.TestType2, D.ItemNameType, cast(C.Spare as int) as Spare FROM   
                            (select SerialNumber, tDateTime, Result, Spare, TestType,TestType2 FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock)   
                            where  tdatetime between ''' + thisdatetime+ " and " + lastdatetime+''' and  itemnametype = '7778')C  
                            inner join    
                            (select ItemNameType, SerialNumber  FROM[ate_db_tblfinal_new].[dbo].[TblFinal]where TestType2 = 183)D    
                            on C.SerialNumber = D.SerialNumber)A 
                            inner join(select itemnametype, ItemDescription from[ate_result].[dbo].[ItemName])B 
                            on A.ItemNameType = B.ItemNameType ) finaltable group by ItemDescription,TestType,TestType2 order by ItemDescription desc'''
            elif Data_type =="AirTight":
                _script =   '''select finaltable.ItemDescription as productname,finaltable.TestType,finaltable.TestType2,sum(finaltable.Result) as RealOutput,AVG(Cast(Spare as int)) as AvgSpare ,
                            (cast(NULLIF(Datediff(MINUTE,MIN(finaltable.tDateTime),MAX(finaltable.tDateTime)),0)as float)/60)as EstimateHours,COUNT(*)/(cast(NULLIF(Datediff(MINUTE,MIN(finaltable.tDateTime),MAX(finaltable.tDateTime)),0)as float)/60) AS EstimateUPH 
                            from (select A.ESN,A.Result,A.TestType,A.tDateTime,REPLACE(B.ItemDescription, 'RTESN', 'Airtight')  AS ItemDescription,A.Spare ,TestType2 FROM   
                            (Select distinct C.SerialNumber AS ESN, C.tDateTime, C.Result, C.TestType,C.TestType2, D.ItemNameType, C.Spare FROM    
                            (select SerialNumber, tDateTime, Result, Spare, TestType,TestType2 FROM[ate_db_tblfinal_new].[dbo].[TblFinal] with(nolock)    
                            where  tdatetime between  ''' + thisdatetime+ " and " + lastdatetime+''' and itemnametype ='8837')C    
                            inner join    
                            (select ItemNameType, SerialNumber,Spare  FROM[ate_db_tblfinal_new].[dbo].[TblFinal]where TestType2 = 183)D    
                            on C.SerialNumber = D.SerialNumber)A    
                            inner join(select itemnametype, ItemDescription from[ate_result].[dbo].[ItemName])B    
                            on A.ItemNameType = B.ItemNameType ) finaltable group by ItemDescription,TestType,TestType2 order by ItemDescription desc'''
            else:
                _script =''
            return _script
                 


    # def UpdateAll(self):

    # def CompareAll(self):
        #return need Update Machine Dict

    # def MappingBarcodeAndMachineID(self):
    #     return AllMappingList


    