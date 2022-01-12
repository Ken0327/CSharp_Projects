import requests
import json
import pandas as pd
import pymsteams
import os
from datetime import datetime, timedelta
import configparser

#======================= using pymsteams api ============================
#ref:https://pypi.org/project/pymsteams/
#Formatting cards with Markdown ref: https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-format?tabs=adaptive-md%2Cconnector-html

def send_teams_notify_PTEMember(df_data):

    #read the config.ini
    wd = os.getcwd()
    config = configparser.ConfigParser()
    config.read(wd + './config.ini')
    url_list = config['webhook_PTE_list']

    time_now = datetime.now()
    pre_time = datetime.now() - timedelta(hours=1)

    # get Owner list
    Owner_list = list(df_data.drop_duplicates(subset=['Empid'], keep='first')['Empid'])

    for i, owner in enumerate(Owner_list):
        tmp_data = df_data[(df_data['Empid'] == owner)]

        if len(tmp_data) > 0:
            # create the connectorcard object with the Microsoft Webhook URL
            myTeamsMessage = pymsteams.connectorcard(url_list[str(owner)], verify=False)
            # Add text to be the title
            title = '**{}:00 ~ {}:00 tracking of {}**'.format(pre_time.strftime('%Y-%m-%d %H'), time_now.strftime('%H'),
                                                              owner)
            myTeamsMessage.text(title)

            for i in range(len(tmp_data)):
                # create section and send the result
                st = pymsteams.cardsection()

                ### using activity section
                st.activityTitle("**{} ({})**".format(tmp_data.iloc[i]['productname'].replace('_', '\_'),
                                                      tmp_data.iloc[i]['ItemNameType']))
                if round(tmp_data.iloc[i]['FYR'] * 100, 2) < 92:
                    st.activityImage("https://img.onl/2SZxWh")  # red
                else:
                    st.activityImage("https://img.onl/SB6cyI")  # green
                st.activitySubtitle('FYR: {}%, Spare: {}s (before: {}s), Output: {}pcs, Esti_uph: {}, ManP: {}'.format(
                    round(tmp_data.iloc[i]['FYR'] * 100, 2), int(tmp_data.iloc[i]['AvgSpare']),
                    tmp_data.iloc[i]['Before_Spare'], int(tmp_data.iloc[i]['RealOutput']),
                    int(tmp_data.iloc[i]['EstimateUPH']),
                    int(tmp_data.iloc[i]['real_manpower'])))

                # if this job is by so = 9999:
                if tmp_data.iloc[i]['PM_uph'] == '999':
                    st.activityText('Note! this is by so = 9999999')
                # a normal test result:
                else:
                    st.activityText('PM_info (Uph: {}, ManP: {})'.format(
                    int(tmp_data.iloc[i]['PM_uph']), int(tmp_data.iloc[i]['PM_manpower'])))
                ### using activity section
                myTeamsMessage.addSection(st)

            # add the detail PTEWEB real time UPH web link: http://peweb/home/PTE_Web/RealTime/RealTimeUPHPerformance
            st = pymsteams.cardsection()
            # st.activitySubtitle(' ')
            myTeamsMessage.addSection(st)
            myTeamsMessage.addLinkButton("View details on PTE_WEB",
                                         config['PTEWEB_URL']['url'])
            print('====' * 25)
            # send the result
            myTeamsMessage.send()