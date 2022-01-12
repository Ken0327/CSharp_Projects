import sys
import pandas
import pyodbc
import DaemonConfig
from Connection import SQLProcess,ConnectionString
import datetime
import DataProcess
sys.setrecursionlimit(1000000)

# Step 0: Date time trigger
# Step 1: Get program config (orglist/maillist/shift)
# Step 2: Get Totlly test data between shift start time and Now
# Step 3: Pre-process the data 
    #ITMXP Group
        #Get Hourly production data - ITMXP
        #Mapping Description
        #Mapping TestType/TestType2
        #Mapping GPN
        #Mapping POH
    #Non-ITMXP
        #Get Hourly production data - ['Athena','Utube','Baro','AirTight']
        #Mapping ItemNameType and Ref GPN        
        #Mapping POH
# Step 4: Upload all realtime data
# Sleep thread


def UploadinfJob(TimeIndex,config,shiftid):
    for org in config.OrgList:
        print('=======================================================================')
        print(org)
        DB = SQLProcess.SqlProcess(org)
        process = DataProcess.Process(org)
        for _thistime in TimeIndex:
            print(_thistime)

            Assy = process.ProcessBasicInfo(org,_thistime,'tblfinal')
            print('ASSY Insert......')
            print(Assy)
            print(DB.InsertDataToRealOutputTable(Assy,shiftid))

            SMT = process.ProcessBasicInfo(org,_thistime,'tblcpu') 
            print(SMT)
            print('SMT Insert......')
            print(DB.InsertDataToRealOutputTable(SMT,shiftid))

            NonITMXPList = ['Athena','uTube','Baro','AirTight']

            for item in NonITMXPList:
                NonITMXP = process.ProcessBasicInfo_NoITMXP(org,_thistime,item)           
                print(NonITMXP)
                print('NonITMXP- %s Insert......'%(item))
                print(DB.InsertDataToRealOutputTable(NonITMXP,shiftid))
        print('Insert Over')


if __name__ == '__main__':
    config = DaemonConfig.Config()
    shift = config.GetShiftByExecuteTime()
    today = config.GetWorkDate()
    TimeIndex_Now = [datetime.datetime(today.year,today.month,today.day,today.hour-9,0,0)]
    print("Now Shift Time index: %s"%(TimeIndex_Now))    
    UploadinfJob(TimeIndex_Now,config,shift)

    # for org in config.OrgList:
    #     print('=======================================================================')
    #     print(org)
    #     DB = SQLProcess.SqlProcess(org)
    #     process = DataProcess.Process(org)
    #     for _thistime in TimeIndex:
    #         print(_thistime)
    #         Assy = process.ProcessBasicInfo(org,_thistime,'tblfinal')
    #         SMT = process.ProcessBasicInfo(org,_thistime,'tblcpu')            
    #         print(Assy)
    #         print(SMT)
    #         print('ASSY Insert......')
    #         print(DB.InsertDataToRealOutputTable(Assy))
    #         print('SMT Insert......')
    #         print(DB.InsertDataToRealOutputTable(SMT))
    #     print('Insert Over')