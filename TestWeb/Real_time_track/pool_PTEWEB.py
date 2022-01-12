import pymssql
import pandas as pd
import time
from datetime import datetime, timedelta
import os
import mysql.connector
import requests
import json
import configparser

# ==== read the config.ini ====
wd = os.getcwd()
config = configparser.ConfigParser()
config.read(wd + './config.ini')

# to compute the real uph(real output pcs) in previous 1 hr and the real manpower
def compute_uph(itemtype, itemstation, conn):

    time_now = datetime.now() - timedelta(hours = 8)
    t_begin = time_now - timedelta(hours = 1)
    t_begin = t_begin.strftime('%Y-%m-%d %H:%M:%S')
    entered_date = time_now.strftime('%Y-%m-%d %H:%M:%S')

    DB = 'tblcpu' if itemstation == 0 else 'tblfinal'

    sql_1 = '''select username, so, station,  count(*) as 'total' from {} where
             tdatetime between '{}' and '{}' and ItemNameType = '{}'
             group by username, so, testtype, testtype2, station'''.format(DB, t_begin, entered_date, itemtype)

    df_data = pd.read_sql(sql_1,conn)
    df_data[['total']] = df_data[['total']].astype(float)
    data_u = df_data.groupby('username').sum()

    real_uph = data_u['total'].sum()
    real_MP = len(data_u)
    print('過去一小時 UPH: ', real_uph, '線上實際人力: ', real_MP)
    return real_uph, real_MP


def get_before(itemtype, conn, timestamp):
    print(timestamp)
    #time_today = datetime.now().strftime('%Y-%m-%d')
    sql_1 = '''select Avg(t.FYR) as 'Before_FYR', Avg(t.Avg_Pass_Time) as 'Before_Spare', sum(t.D_Total) 'DTotal' 
        from (select top 8 FYR, Avg_Pass_Time, D_Total from PTEDB.dbo.PTEWEB_ItemNameType_ByDaily
        where ItemNameType = '{}' and date < '{}' 
        order by date desc) as t'''.format(itemtype, timestamp)

    df_data = pd.read_sql(sql_1, conn)
    return df_data

def target_PTEWEB():
    conn = pymssql.connect(
        host = config['PTEWEB_DB']['mssql_host'],
        user = config['PTEWEB_DB']['mssql_username'],
        password = config['PTEWEB_DB']['mssql_password'],
        database = config['PTEWEB_DB']['mssql_database']
    )

    # pre_time: 1 hour ago
    pre_time = datetime.now() - timedelta(hours=1)
    # get time index
    t_index = int(pre_time.strftime('%H'))
    t_date = pre_time.strftime('%Y-%m-%d')


    sql_1 = '''select * from PTEDB.dbo.PTEWEB_RealTimeTrack as A
        left join (select * from PTEDB.dbo.PTEWEB_ItemNameType_RealOutput
        where date is not null and date = '{}' and TimeIndex = '{}') as B
        on A.ItemNameType = B.itemnametype and A.Issue_Org = B.Org'''.format(t_date, t_index)

    df_data = pd.read_sql(sql_1, conn)

    ### set a lower realoutput limit > 60 pcs
    df_data = df_data[df_data['RealOutput'] > 60]
    df_data.reset_index(drop=True)
    return (df_data, conn)

#determine the connection to which Org (by the input string Org)
def conn_checkOrg(Org):
    if Org.upper() == 'T1':
        conn = pymssql.connect(
            host = config['T1_DB']['mssql_host'],
            user = config['T1_DB']['mssql_username'],
            password = config['T1_DB']['mssql_password'],
            database = config['T1_DB']['mssql_database']
            )
    elif Org.upper() == 'T2':
        conn = pymssql.connect(
            host = config['T2_DB']['mssql_host'],
            user = config['T2_DB']['mssql_username'],
            password = config['T2_DB']['mssql_password'],
            database = config['T2_DB']['mssql_database']
            )
    elif Org.upper() == 'T3':
        conn = pymssql.connect(
            host = config['T3_DB']['mssql_host'],
            user = config['T3_DB']['mssql_username'],
            password = config['T3_DB']['mssql_password'],
            database = config['T3_DB']['mssql_database']
            )
    return conn

#check whether the itmxp is ATE or ASSY
def check_itmxp_station(itemtype):
    ITMXPdb = mysql.connector.connect(
        host = config['ITMXP_DB']['mysql_host'],
        user = config['ITMXP_DB']['mysql_username'],
        password = config['ITMXP_DB']['mysql_password'],
        database = config['ITMXP_DB']['mysql_database']
    )

    sql = '''SELECT a.unikey, b.idx_file, b.itemnametype, a.xmlname, a.stationtype
    FROM itmxp.testfile as a
    left join itmxp.tbl_testinfo as b on a.unikey = b.idx_file
    where b.itemnametype = '{}' '''.format(itemtype)

    df_data = pd.read_sql_query(sql, ITMXPdb)
    return int(df_data['stationtype'])

# check the largest MP SO in previous 1 hour
def check_status(itemtype, itemstation, conn):

    #time_now: current time GMT+0
    time_now = datetime.now() - timedelta(hours = 8)
    t_begin = time_now - timedelta(hours = 1)
    t_beg = t_begin.strftime('%Y-%m-%d %H:%M:%S')
    t_end = time_now.strftime('%Y-%m-%d %H:%M:%S')

    DB = 'tblcpu' if itemstation == 0 else 'tblfinal'

    sql_1 = '''select username, so, testtype, testtype2,station, exeinfo,  count(*) as 'total' from {} where
         tdatetime between '{}' and '{}' and ItemNameType = '{}'
         group by username, so, testtype, testtype2, station, exeinfo'''.format(DB, t_beg, t_end, itemtype)
    df_data = pd.read_sql(sql_1,conn)

    title = str(df_data['exeinfo'][0]).split("[")[2].split(" ")[0]
    tp1 = df_data['testtype'][0]
    tp2 = 'NA' if df_data['testtype2'][0] == '' else df_data['testtype2'][0]
    return tp1, tp2, int(df_data[df_data['total'] == df_data['total'].max()]['so'].iloc[0]), df_data['total'].sum(), title

# get_PM_info: get the production map info of this itemnametype by input so and testtype
def get_PM_info(so_id, tp1, tp2):
    url = config['PMinfo_url']['url'] #'http://linwpa-pi01.ad.garmin.com/DotNetFrameworkAPI/api/Jobinfo'
    data = {"soid": so_id, "side": ""}; body = json.loads(json.dumps(data))
    res = requests.post(url, json=body)
    content = json.loads(res.text)
    PM_test = 'NAN'; PM_uph = 'NAN'
    Prod_name = content['Payload']['ItemDescript']
    PM_GPN = content['Payload']['GPN012']
    ### get the uph by testtype
    for i, info in enumerate(content['Payload']['ProductionMap']):
        if info['TestType'] == tp1 and info['TestType2'] == tp2:
            PM_st = info['Description']
            PM_uph = info['CAPA_POH']
            PM_MAN = info["Manpower"]
    if PM_uph == 'NAN':
        print('no matching testtype in PuctMap', {'testtype': (tp1, tp2)})
        return 'NAN', 'NAN', 'NAN', '0', '0'
    else:
        print('======= ProductionMap info =======')
        print('Product: {}\nJob_SO: {}\nGPN: {}\nthis station: {}\nuph: {}\nman power: {}'.format(
                Prod_name, so_id, PM_GPN, PM_st, PM_uph, PM_MAN))
        print('==================================')
        return Prod_name,PM_GPN, PM_st, PM_uph, PM_MAN
