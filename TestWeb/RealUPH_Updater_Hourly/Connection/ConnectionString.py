import sys

class Connection():
    def __init__(self):
        #self.C5_MGU_Conn = 'DRIVER={ODBC Driver 13 for SQL Server};SERVER=YANWPD-ATESQLR;DATABASE=MGUDB;UID=ate_oper;PWD=ate.oper'
        self.T1_MGU_Conn = 'DRIVER={SQL Server};SERVER=SHIWPD-ATESQLR;DATABASE=ate_db;UID=ate_oper;PWD=ate.oper'
        self.T2_MGU_Conn = 'DRIVER={SQL Server};SERVER=JHOWPD-ATESQLR;DATABASE=ate_db;UID=ate_oper;PWD=ate.oper'
        self.T3_MGU_Conn = 'DRIVER={SQL Server};SERVER=LINWPD-ATESQLR;DATABASE=ate_db;UID=ate_oper;PWD=ate.oper'
        self.T5_MGU_Conn = 'DRIVER={SQL Server};SERVER=XINWPD-ATESQLR;DATABASE=ate_db;UID=ate_oper;PWD=ate.oper' 
        self.PE_Support_Conn = 'DRIVER={SQL Server};SERVER=t1-pe-support\\pesupport;DATABASE=PTEDB;UID=PIENG;PWD=Q2iT5cwHJW3FH'