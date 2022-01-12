import pandas as pd
import numpy as np
from Connection import SQLProcess,ConnectionString
import datetime

class Process(object):
    def __init__(self,org):
        self.SqlProcess = SQLProcess.SqlProcess(org)

    def ProcessBasicInfo(self,org,_thistime,table):
        Output = self.SqlProcess.GetItemNameTypeByHour(org,table,_thistime,1)
        Output['Org'] = org
        Output['TimeIndex'] = (_thistime + datetime.timedelta(hours=8)).hour
        result = self.ProcessDescription(Output,_thistime,table)
        # result = self.ProcessTestOutput(Output,_thistime,table)
        return result

    def ProcessBasicInfo_NoITMXP(self,org,_thistime,data_type):
        Output = self.SqlProcess.GetNoITMXPDataByHour(org,_thistime,1,data_type)
        Output['Org'] = org
        Output['TimeIndex'] = (_thistime + datetime.timedelta(hours=8)).hour
        result = self.ProcessDescription_NonITMXP(Output,_thistime)
        # result = self.ProcessTestOutput(Output,_thistime,table)
        return result

    def ProcessDescription_NonITMXP(self,_input,_thistime):
        ItemDescList = _input['productname']
        if ItemDescList.size == 0:
            return None   
        MappingTable_ItemNameType = self.SqlProcess.GetItemNameType_NonITMPXP(ItemDescList)
        Output = pd.merge(_input,MappingTable_ItemNameType,on=['productname'])
        MappingTable_gpn = self.SqlProcess.GetGPNByProductName(Output) 
        Output = pd.merge(Output,MappingTable_gpn,on=['productname'])   
        MappingTable_UPH = self.SqlProcess.GetUPHByProductNamw_NonITMXP(Output)  
        Output = pd.merge(Output,MappingTable_UPH,on=['productname'])  
        return Output

    #bk PASS
    # def ProcessDescription_NonITMXP(self,_input,_thistime):
    #     ItemDescList = _input['productname']
    #     if ItemDescList.size == 0:
    #         return None   
    #     MappingTable_ItemNameType = self.SqlProcess.GetItemNameType_NonITMPXP(ItemDescList)
    #     Output = pd.merge(_input,MappingTable_ItemNameType,on=['productname'])    
    #     MappingTable_UPH = self.SqlProcess.GetUPHByProductNamw_NonITMXP(Output)  
    #     Output = pd.merge(Output,MappingTable_UPH,on=['productname'])  
    #     Output['gpn'] = 'xxx-xxxxx-xx' 
    #     return Output


    def ProcessDescription(self,_input,_thistime,table):
        itemlist = _input['ItemNameType']
        if itemlist.size == 0:
            return None
        print(itemlist)
        MappingTable_Description = self.SqlProcess.GetDescriptionByItemNameType(itemlist)
        # print(MappingTable_Description.count)
        MappingTable_TestType = self.SqlProcess.GetTestTypeByItemNameType(itemlist)
        # print(MappingTable_TestType.count)
        Mapping_GPN = self.SqlProcess.GetGPNByItemNameType(table,_thistime,itemlist)
        # print(Mapping_GPN.count)
        Output = pd.merge(_input,MappingTable_Description,on=['ItemNameType'])
        # print(Output)
        Output = pd.merge(Output,MappingTable_TestType,on=['ItemNameType'])
        print(Output)
        Output = pd.merge(Output,Mapping_GPN,on=['ItemNameType'])
        # print(Output)

        Mapping_uph = self.SqlProcess.GetUPHFromProductionMap(Output)
        Output =pd.merge(Output,Mapping_uph,on=['ItemNameType'])
        # print(Output)
        Mapping_RealOutput = self.SqlProcess.GetRealOutputByItemNameType(table,_thistime,itemlist)
        Output =pd.merge(Output,Mapping_RealOutput,on=['ItemNameType'])
        
        Mapping_FYR = self.SqlProcess.GetFYRByItemNameType(table,_thistime,itemlist)
        Output =pd.merge(Output,Mapping_FYR,on=['ItemNameType'])
        
        print(Output)
        return Output

    def ProcessTestOutput(self,_input,_thistime,table):
        try:

            return None
        except:
            return _input

            

