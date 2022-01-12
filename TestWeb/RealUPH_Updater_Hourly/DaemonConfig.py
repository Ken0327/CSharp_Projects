import datetime

class Config():
    OrgList = ['T1','T2','T3','T5']    
    # frequncy : second
    UpdateFrequency = 3600
    ExcepttionHandlerMailList = ['Jasper.Fang@garmin.com','Ken.Chen@garmin.com','Chris.Feng@garmin.com']
    
    def GetShiftByExecuteTime(self):
        NowTime = datetime.datetime.now()
        if NowTime.hour >= 8 and NowTime.hour <=20 :
            shift =  0
        else:
            shift =  1
        return shift

    def GetWorkDate(self):
        today = datetime.datetime.now()
        if today.hour < 8 :
            today = today +  datetime.timedelta(days=-1)  
        return today


    def GetTimeIndexByShift(self,shift,thistime):
        datetimerange = []
        if shift == 0:
            if thistime.hour >=8 and thistime.hour <= 19 :
                while(thistime.hour!= 7):
                    Upcdatetime = thistime+datetime.timedelta(hours=-8)                    
                    datetimerange.append(datetime.datetime(Upcdatetime.year,Upcdatetime.month,Upcdatetime.day,Upcdatetime.hour,0,0))
                    thistime = thistime + datetime.timedelta(hours=-1)
            else:
                while(thistime.hour!= 19):
                    Upcdatetime = thistime+datetime.timedelta(hours=-8)
                    datetimerange.append(datetime.datetime(Upcdatetime.year,Upcdatetime.month,Upcdatetime.day,Upcdatetime.hour,0,0))
                    thistime = thistime + datetime.timedelta(hours=-1)
        if shift == 1:
            workday = datetime.date.today()
            if thistime.hour >=8 and thistime.hour <20: #day shift
                workdatetime = datetime.datetime(workday.year,workday.month,workday.day,7,0,0)
                while(workdatetime.hour != 19):
                    Upcdatetime = workdatetime+datetime.timedelta(hours=-8)
                    datetimerange.append(datetime.datetime(Upcdatetime.year,Upcdatetime.month,Upcdatetime.day,Upcdatetime.hour,0,0))
                    workdatetime = workdatetime + datetime.timedelta(hours=-1)
            else: #night
                if thistime.hour < 19 : #night shift
                    workday = workday + datetime.timedelta(days=-1)
                workdatetime = datetime.datetime(workday.year,workday.month,workday.day,19,0,0)                    
                while(workdatetime.hour!=7):
                    Upcdatetime = workdatetime+datetime.timedelta(hours=-8)                    
                    datetimerange.append(datetime.datetime(Upcdatetime.year,Upcdatetime.month,Upcdatetime.day,Upcdatetime.hour,0,0))
                    workdatetime = workdatetime + datetime.timedelta(hours=-1)
        return datetimerange

            
