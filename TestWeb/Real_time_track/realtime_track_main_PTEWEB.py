import pymssql
import pandas as pd
import matplotlib.pyplot as plt
import time
from datetime import datetime, timedelta
import os
from pool_PTEWEB import *
from teams_notify_PTEWEB import *

# ==== main script ====
def mainscript(df_tarlist, conn_PTEWEB):
    real_mp_list = []; uput_PM_uph_list = []; PM_manpower_list = []
    before_spare_list = []

    for i, input_itemnametype in enumerate(df_tarlist['ItemNameType']):
        #check itemnametype stationtype (ATE or ASSY)
        itemstation = check_itmxp_station(itemtype=input_itemnametype)

        #check the Org and set the connection of Server
        connection = conn_checkOrg(Org = df_tarlist.iloc[i]['Issue_Org'])

        status_info = check_status(itemtype = input_itemnametype, itemstation = itemstation, conn = connection)
        df_before = get_before(itemtype = input_itemnametype, conn=conn_PTEWEB, timestamp = str(df_tarlist.iloc[i]['Timestamp'])[0:10])
        # status_info[2] == '9999999' which means so = 9999999
        if str(status_info[2]) == '9999999':
            message = 'itemnametype: {} ({}), realoutput: {} (by SO = 9999)'.format(input_itemnametype,
                                            df_tarlist['productname'].iloc[i], int(df_tarlist['RealOutput'].iloc[i]))

            # compute previous one hours uph and manpower via function compute_uph:
            real_uph, real_MP = compute_uph(itemtype=input_itemnametype, itemstation=itemstation, conn=connection)
            #print(message)

            real_mp_list.append(int(real_MP)); uput_PM_uph_list.append('999'); PM_manpower_list.append('999')
            before_spare_list.append(int(df_before['Before_Spare']))

        else:
            (uput_Prod_name, uput_PM_GPN, uput_PM_st, uput_PM_uph, uput_PM_MAN) = get_PM_info(so_id = status_info[2], tp1 = status_info[0], tp2= status_info[1])
            #compute previous one hours uph via function compute_uph:
            real_uph, real_MP = compute_uph(itemtype=input_itemnametype, itemstation = itemstation, conn = connection)

            message = '''itemtype: {} ({}) ==> last 1 hour: {} pcs, FYR: {}%, Spare: {}(s), uph on ProductMap: {}'''.format(input_itemnametype,
            df_tarlist['productname'].iloc[i], df_tarlist['RealOutput'].iloc[i],
            round(df_tarlist['FYR'].iloc[i]*100,2), df_tarlist['AvgSpare'].iloc[i], uput_PM_uph)

            # =====send message via line notify
            #print('Notify: {}'.format(message))

            ### =====================
            real_mp_list.append(int(real_MP))
            uput_PM_uph_list.append(int(uput_PM_uph))
            PM_manpower_list.append(int(uput_PM_MAN))
            before_spare_list.append(int(df_before['Before_Spare']))
            #=============== output the last hour production result!
        #print('#####'*25)

    ### === collect data
    df_tarlist['real_manpower'] = pd.Series(real_mp_list, index=df_tarlist.index)
    df_tarlist['PM_uph'] = pd.Series(uput_PM_uph_list, index=df_tarlist.index)
    df_tarlist['PM_manpower'] = pd.Series(PM_manpower_list, index=df_tarlist.index)
    df_tarlist['Before_Spare'] = pd.Series(before_spare_list, index=df_tarlist.index)

    # get now direction
    #wd = os.getcwd()

    #to save the target list (may occur permission denied to save this file on server)
    #df_tarlist.to_csv(wd +'./df_tarlist.csv', encoding='big5')

    #return the final data frame
    return df_tarlist

if __name__ == "__main__":

    #get the input from PTEWEB
    (df_tarlist, conn_PTEWEB) = target_PTEWEB()

    # detect at least one product is running
    if len(df_tarlist) > 0:
        print(df_tarlist)
        #mainscript() function to deal with all the funciton and return the result dataframe, input is the target list
        final_df = mainscript(df_tarlist = df_tarlist, conn_PTEWEB = conn_PTEWEB)

        #using send_teams_notify funciton to send the notify to owners, input is the result dataframe
        send_teams_notify_PTEMember(df_data = final_df)

