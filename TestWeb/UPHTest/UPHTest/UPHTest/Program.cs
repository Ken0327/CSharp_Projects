using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms.DataVisualization.Charting;

namespace UPHTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string sql = $@"  SELECT A.*,B.Total FROM
  (SELECT *  FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily]   where  UPH!=999 and RealOutput>='1000'  and date between '{DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")}'and '{DateTime.Now.ToString("yyyy-MM-dd")}' and Org='T1'
                                      UNION ALL
                                      SELECT* FROM[PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where UPH!=999 and RealOutput>='1000'  and date between '{DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")}'and '{DateTime.Now.ToString("yyyy-MM-dd")}' and Org = 'T2'
                                      UNION ALL
                                      SELECT* FROM[PTEDB].[dbo].[PTEWEB_ItemNameType_RealOutput_ByDaily] where UPH!=999 and RealOutput>='500'  and date between '{DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")}'and '{DateTime.Now.ToString("yyyy-MM-dd")}' and Org = 'T3')A
									  left join
									 (SELECT * FROM [PTEDB].[dbo].[PTEWEB_ItemNameType_ByDaily] where Date='{DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")}' UNION ALL
									  SELECT * FROM [PTEDB].[dbo].[PTEWEB_nonITMXP_ByDaily] where Date='{DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")}'
									 )B on A.Org=B.Org and A.ItemNameType=B.ItemNameType";
            DataTable dt = new DataTable();
            DbConn dc = new DbConn();

            dt = dc.GetTableFromSql(sql, dc.PTEDB_ConnString);

            if (dt.Rows.Count > 0)
            {
                dt = dt.AsEnumerable().Where(p => p.IsNull("Total") == false).Select(x => x).CopyToDataTable();
                var GroupOrg = from r in dt.AsEnumerable()
                               group r by new
                               {
                                   Org = r.Field<string>("Org"),
                                   ItemNameType = r.Field<int>("ItemNameType")
                               } into g
                               select new
                               {
                                   GroupOrg = g.Key.Org,
                                   GroupItemNameType = g.Key.ItemNameType,
                                   Item = g.ToList()
                               };

                var FilterProduct = from g in GroupOrg.AsEnumerable()
                                    select new
                                    {
                                        org = g.GroupOrg,
                                        ItemNameType = g.GroupItemNameType,
                                        Product = g.Item[0].Field<string>("ProductName"),
                                        Achievement = Math.Round(g.Item.AsEnumerable().Average(x => x.Field<double>("EstimateUPH") / x.Field<int>("UPH")) * 100, 3),
                                        Count = g.Item[0].Field<int>("Total")
                                    };
                var OrgsData = from o in FilterProduct.AsEnumerable()
                               group o by o.org into g
                               select g;
                SendEmail se = new SendEmail();
                // IWorkbook workbook = new XSSFWorkbook();
                HSSFWorkbook workbook = new HSSFWorkbook();
                // Use workbook to Create Sheet
                foreach (var o in OrgsData)
                {
                    HSSFSheet sheet = (HSSFSheet)workbook.CreateSheet(o.Key + "(" + o.ToList().Count() + ")");
                    int i = 0;
                    var plow70 = o.ToList().Where(x => x.Achievement < 70).OrderBy(x => x.Achievement);
                    var plarge70low80 = o.ToList().Where(x => x.Achievement >= 70 && x.Achievement < 80).OrderBy(x => x.Achievement);
                    var plarge80low90 = o.ToList().Where(x => x.Achievement >= 80 && x.Achievement < 90).OrderBy(x => x.Achievement);
                    var plarge90low100 = o.ToList().Where(x => x.Achievement >= 90 && x.Achievement < 100).OrderBy(x => x.Achievement);

                    var plarge100 = o.ToList().Where(x => x.Achievement >= 100).OrderBy(x => x.Achievement);

                    sheet.CreateRow(i).CreateCell(0).SetCellValue(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "~" + DateTime.Now.ToString("yyyy-MM-dd") + " UPH 數據");

                    //建立跨越五列(共六列 0~1)  ，跨越三欄(共四欄 0-2)
                    sheet.AddMergedRegion(new CellRangeAddress(i, i + 1, 0, 2));
                    i += 2;
                    HSSFCellStyle style = (HSSFCellStyle)workbook.CreateCellStyle();
                    style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;

                    style.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;

                    HSSFRow row = (HSSFRow)sheet.CreateRow(i);
                    HSSFCell cell = (HSSFCell)row.CreateCell(0);

                    cell.CellStyle = style;
                    cell.SetCellValue("70%~80%");

                    // sheet.CreateRow(i).CreateCell(0).SetCellValue("70%~80%");

                    //建立跨越五列(共六列 0~1)  ，跨越三欄(共四欄 0-2)
                    sheet.AddMergedRegion(new CellRangeAddress(i, i + 1, 0, 3));
                    i += 2;
                    sheet.CreateRow(i);
                    sheet.GetRow(i).CreateCell(0).SetCellValue("ItemNameType");
                    sheet.GetRow(i).CreateCell(1).SetCellValue("Product Name");
                    sheet.GetRow(i).CreateCell(2).SetCellValue("Achievement(%)");
                    sheet.GetRow(i).CreateCell(3).SetCellValue("Count");
                    i++;
                    foreach (var p in plarge70low80)
                    {
                        sheet.CreateRow(i);
                        sheet.GetRow(i).CreateCell(0).SetCellValue(p.ItemNameType);
                        sheet.GetRow(i).CreateCell(1).SetCellValue(p.Product);
                        sheet.GetRow(i).CreateCell(2).SetCellValue(p.Achievement);
                        sheet.GetRow(i).CreateCell(3).SetCellValue(p.Count);
                        i++;
                    }

                    style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;

                    style.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;

                    row = (HSSFRow)sheet.CreateRow(i);
                    cell = (HSSFCell)row.CreateCell(0);

                    cell.CellStyle = style;
                    cell.SetCellValue("80%~90%");

                    sheet.AddMergedRegion(new CellRangeAddress(i, i + 1, 0, 3));
                    i++;
                    sheet.CreateRow(i);
                    sheet.GetRow(i).CreateCell(0).SetCellValue("ItemNameType");
                    sheet.GetRow(i).CreateCell(1).SetCellValue("Product Name");
                    sheet.GetRow(i).CreateCell(2).SetCellValue("Achievement(%)");
                    sheet.GetRow(i).CreateCell(3).SetCellValue("Count(%)");
                    i++;
                    foreach (var p in plarge80low90)
                    {
                        sheet.CreateRow(i);
                        sheet.GetRow(i).CreateCell(0).SetCellValue(p.ItemNameType);
                        sheet.GetRow(i).CreateCell(1).SetCellValue(p.Product);
                        sheet.GetRow(i).CreateCell(2).SetCellValue(p.Achievement);
                        sheet.GetRow(i).CreateCell(3).SetCellValue(p.Count);
                        i++;
                    }
                    style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;

                    style.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;

                    row = (HSSFRow)sheet.CreateRow(i);
                    cell = (HSSFCell)row.CreateCell(0);

                    cell.CellStyle = style;
                    cell.SetCellValue("90%~100%");
                    sheet.AddMergedRegion(new CellRangeAddress(i, i + 1, 0, 3));
                    i++;
                    sheet.CreateRow(i);
                    sheet.GetRow(i).CreateCell(0).SetCellValue("ItemNameType");
                    sheet.GetRow(i).CreateCell(1).SetCellValue("Product Name");
                    sheet.GetRow(i).CreateCell(2).SetCellValue("Achievement(%)");
                    sheet.GetRow(i).CreateCell(3).SetCellValue("Count(%)");
                    i++;
                    foreach (var p in plarge90low100)
                    {
                        sheet.CreateRow(i);
                        sheet.GetRow(i).CreateCell(0).SetCellValue(p.ItemNameType);
                        sheet.GetRow(i).CreateCell(1).SetCellValue(p.Product);
                        sheet.GetRow(i).CreateCell(2).SetCellValue(p.Achievement);
                        sheet.GetRow(i).CreateCell(3).SetCellValue(p.Count);
                        i++;
                    }
                    style = (HSSFCellStyle)workbook.CreateCellStyle();
                    style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightBlue.Index;

                    style.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;

                    row = (HSSFRow)sheet.CreateRow(i);
                    cell = (HSSFCell)row.CreateCell(0);

                    cell.CellStyle = style;
                    cell.SetCellValue("UPH大於100%");
                    sheet.AddMergedRegion(new CellRangeAddress(i, i + 1, 0, 3));
                    i++;
                    sheet.CreateRow(i);
                    sheet.GetRow(i).CreateCell(0).SetCellValue("ItemNameType");
                    sheet.GetRow(i).CreateCell(1).SetCellValue("Product Name");
                    sheet.GetRow(i).CreateCell(2).SetCellValue("Achievement(%)");
                    sheet.GetRow(i).CreateCell(3).SetCellValue("Count(%)");
                    i++;
                    foreach (var p in plarge100)
                    {
                        sheet.CreateRow(i);
                        sheet.GetRow(i).CreateCell(0).SetCellValue(p.ItemNameType);
                        sheet.GetRow(i).CreateCell(1).SetCellValue(p.Product);
                        sheet.GetRow(i).CreateCell(2).SetCellValue(p.Achievement);
                        sheet.GetRow(i).CreateCell(3).SetCellValue(p.Count);
                        i++;
                    }
                    style = (HSSFCellStyle)workbook.CreateCellStyle();
                    style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Pink.Index;

                    style.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;

                    row = (HSSFRow)sheet.CreateRow(i);
                    cell = (HSSFCell)row.CreateCell(0);

                    cell.CellStyle = style;
                    cell.SetCellValue("UPH小於70%");
                    sheet.AddMergedRegion(new CellRangeAddress(i, i + 1, 0, 3));
                    i++;
                    sheet.CreateRow(i);
                    sheet.GetRow(i).CreateCell(0).SetCellValue("ItemNameType");
                    sheet.GetRow(i).CreateCell(1).SetCellValue("Product Name");
                    sheet.GetRow(i).CreateCell(2).SetCellValue("Achievement(%)");
                    sheet.GetRow(i).CreateCell(3).SetCellValue("Count(%)");
                    i++;
                    foreach (var p in plow70)
                    {
                        sheet.CreateRow(i);
                        sheet.GetRow(i).CreateCell(0).SetCellValue(p.ItemNameType);
                        sheet.GetRow(i).CreateCell(1).SetCellValue(p.Product);
                        sheet.GetRow(i).CreateCell(2).SetCellValue(p.Achievement);
                        sheet.GetRow(i).CreateCell(3).SetCellValue(p.Count);
                        i++;
                    }
                    var plow80 = o.ToList().Where(x => x.Achievement < 80).OrderBy(x => x.Achievement);
                    foreach (var p in plow70)
                    {
                        ProcessIssue(p.ItemNameType, p.org);
                    }
                }
                // Call MemoryStream to Write it
                NpoiMemoryStream ms = new NpoiMemoryStream();
                ms.AllowClose = false;
                workbook.Write(ms);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                se.Attatch = WrapExcelBytesInAttachment(ms);
                se.strSubject = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "UPH資料檔";
                se.strTo = "jerry.hsieh@garmin.com;justin.wu@garmin.com;jasper.fang@garmin.com";
                //  se.strTo = "jerry.hsieh@garmin.com;";
                se.strBody = "請參考附件";
                se.Send();
                ms.AllowClose = true;
            }
        }

        private static void ProcessIssue(int ItemNameType, string Org)
        {
            DbConn conn = new DbConn();
            DateTime today = DateTime.Now;
            string itemSQL = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] where itemNameType='" + ItemNameType + "' and Support_Org='" + Org + "'   order by LastUpdateDate desc";
            DataTable singleItemNameType = conn.GetTableFromSql(itemSQL, conn.PTEDB_ConnString);
            string org = Org;
            int ITM = ItemNameType > 850000 ? 0 : 1;
            //  string QueryItemNameTypeSQL = "Select * FROM [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] where itemNameType='" + ItemNameType + "' and Support_Org='" + org + "' order by LastUpdateDate desc";
            // DataTable ItemNameTypeSearch = conn.GetTableFromSql(QueryItemNameTypeSQL, conn.PTEDB_ConnString);
            if (singleItemNameType.Rows.Count == 0)
            {
                //add new record
                string InsertSQL = "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_ByDaily]([ItemNameType] ,[IssueAlive_Dates] ,[Status],[Support_Org] ,[LastUpdateDate],[FYR_Time],[ITM])" +
                                               "VALUES('" + ItemNameType + "','1','0','" + org + "','" + today.ToString("yyyy-MM-dd") + "','" + today.ToString("yyyy-MM-dd") + "','" + ITM + "');";
                conn.ExecuteFromSql(InsertSQL, conn.PTEDB_ConnString);
                singleItemNameType = conn.GetTableFromSql(itemSQL, conn.PTEDB_ConnString);
            }

            //1.資料表無記錄update itemNameType 新增記錄                               //2.資料表有記錄且issue為close, 轉為open，新增issue記錄
            if (singleItemNameType.Rows[0]["Title_id_UPH"] == DBNull.Value || !Convert.ToBoolean(singleItemNameType.Rows[0]["Status"]))
            {
                if (checkNewTitleData(ItemNameType, org))
                {
                    int insert_id = conn.ExecuteFromSqlReturnid("INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + ItemNameType + "','UPH達成率低於80%','" + 1 + "','" + today.ToString("yyyy-MM-dd") + "','" + org + "');", conn.PTEDB_ConnString);
                    string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',Title_id_UPH='" + insert_id + "',status=1 " +
                                                       " where ItemNameType='" + ItemNameType + "' and Support_Org='" + org + "'; " +
                                                       "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + ItemNameType + "','1','UPH達成率低於80%','" + today.ToString("yyyy-MM-dd") + "','" + org + "');";
                    conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                }
                else
                {
                    int insert_id = conn.ExecuteFromSqlReturnid("select top 1 Title_id from PTEWEB_Issues_Title where itemnametype='" + ItemNameType + "' and org='" + org + "' order by title_id desc", conn.PTEDB_ConnString);
                    string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',Title_id_UPH='" + insert_id + "',status=1 " +
                                                       " where ItemNameType='" + ItemNameType + "' and Support_Org='" + org + "'; " +
                                                       "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + ItemNameType + "','1','UPH達成率低於80%','" + today.ToString("yyyy-MM-dd") + "','" + org + "');UPDATE [PTEDB].[dbo].[PTEWEB_Issues_Title] SET [Title]='UPH達成率低於80%' , [Issue_Status]='1',[CreateDate]='" + today.ToString("yyyy-MM-dd") + "'  where itemnametype='" + ItemNameType + "' and Title_id='" + insert_id + "' and [Org]='" + org.Trim() + "'";
                    conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                }
            }
            else
            {//open issue
                if (checkNewTitleData(ItemNameType, org))
                {
                    int insert_id = conn.ExecuteFromSqlReturnid("INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_Title]( [ItemNameType],[Title],[Issue_Status]  ,[CreateDate],[Org]) output INSERTED.Title_id VALUES('" + ItemNameType + "','UPH達成率低於80%','" + 1 + "','" + today.ToString("yyyy-MM-dd") + "','" + org + "');", conn.PTEDB_ConnString);
                    string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',Title_id_UPH='" + insert_id + "',status=1 " +
                                                  " where ItemNameType='" + ItemNameType + "' and Support_Org='" + org + "'; " +
                                                  " INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + ItemNameType + "','1','UPH達成率低於80%','" + today.ToString("yyyy-MM-dd") + "','" + org + "');";
                    conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                }
                else
                {
                    string UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_Title] SET [Title]='UPH達成率低於80%' , [Issue_Status]='1',[CreateDate]='" + today.ToString("yyyy-MM-dd") + "'  where itemnametype='" + ItemNameType + "' and Title_id='" + Convert.ToInt32(singleItemNameType.Rows[0]["Title_id_UPH"]) + "',[Org]='" + org + "'";
                    conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                    UPDATESQL = "UPDATE [PTEDB].[dbo].[PTEWEB_Issues_ByDaily] set  LastUpdateDate='" + today.ToString("yyyy-MM-dd") + "',status=1 " +
                                             " where ItemNameType='" + ItemNameType + "' and Support_Org='" + org + "'; " +
                                             "INSERT INTO [PTEDB].[dbo].[PTEWEB_Issues_History]([ItemNameType] ,[Status],[Contents],[DateTime],[Org]) VALUES('" + ItemNameType + "','1','UPH達成率低於80%','" + today.ToString("yyyy-MM-dd") + "','" + org + "');";
                    conn.ExecuteFromSql(UPDATESQL, conn.PTEDB_ConnString);
                }
            }
        }

        private static bool checkNewTitleData(int ItemNameType, string TxOrg)
        {
            DbConn conn = new DbConn();
            string SQL = "SELECT Issue_Status FROM [PTEDB].[dbo].[PTEWEB_Issues_Title]where itemNameType='" + ItemNameType + "' and Org='" + TxOrg + "' order by Title_id desc";
            DataTable dt = conn.GetTableFromSql(SQL, conn.PTEDB_ConnString);
            if (dt.Rows.Count > 0)
            {
                return !Convert.ToBoolean(dt.Rows[0]["Issue_Status"]);  //1 returen false->update 0->return true insert
            }
            else
            {
                return !false;//insert new record
            }
        }

        private static Attachment WrapExcelBytesInAttachment(NpoiMemoryStream ms)
        {
            try
            {
                //Stream stream = new MemoryStream(excelContent);
                Attachment attachment = new Attachment(ms, DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "UPH資料檔" + ".xls");
                attachment.ContentType = new ContentType("application/vnd.ms-excel");
                return attachment;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static Attachment CreateAttachment(string attachmentFile, string displayName, TransferEncoding transferEncoding)
        {
            Attachment attachment = new Attachment(attachmentFile);
            attachment.TransferEncoding = transferEncoding;

            string tranferEncodingMarker = String.Empty;
            string encodingMarker = String.Empty;
            int maxChunkLength = 0;

            switch (transferEncoding)
            {
                case TransferEncoding.Base64:
                    tranferEncodingMarker = "B";
                    encodingMarker = "UTF-8";
                    maxChunkLength = 30;
                    break;

                case TransferEncoding.QuotedPrintable:
                    tranferEncodingMarker = "Q";
                    encodingMarker = "ISO-8859-1";
                    maxChunkLength = 76;
                    break;

                default:
                    throw (new ArgumentException(String.Format("The specified TransferEncoding is not supported: {0}", transferEncoding, "transferEncoding")));
            }

            attachment.NameEncoding = Encoding.GetEncoding(encodingMarker);

            string encodingtoken = String.Format("=?{0}?{1}?", encodingMarker, tranferEncodingMarker);
            string softbreak = "?=";
            string encodedAttachmentName = encodingtoken;

            if (attachment.TransferEncoding == TransferEncoding.QuotedPrintable)
                encodedAttachmentName = HttpUtility.UrlEncode(displayName, Encoding.Default).Replace("+", " ").Replace("%", "=");
            else
                encodedAttachmentName = Convert.ToBase64String(Encoding.UTF8.GetBytes(displayName));

            encodedAttachmentName = SplitEncodedAttachmentName(encodingtoken, softbreak, maxChunkLength, encodedAttachmentName);
            attachment.Name = encodedAttachmentName;

            return attachment;
        }

        private static string SplitEncodedAttachmentName(string encodingtoken, string softbreak, int maxChunkLength, string encoded)
        {
            int splitLength = maxChunkLength - encodingtoken.Length - (softbreak.Length * 2);
            var parts = SplitByLength(encoded, splitLength);

            string encodedAttachmentName = encodingtoken;

            foreach (var part in parts)
                encodedAttachmentName += part + softbreak + encodingtoken;

            encodedAttachmentName = encodedAttachmentName.Remove(encodedAttachmentName.Length - encodingtoken.Length, encodingtoken.Length);
            return encodedAttachmentName;
        }

        private static IEnumerable<string> SplitByLength(string stringToSplit, int length)
        {
            while (stringToSplit.Length > length)
            {
                yield return stringToSplit.Substring(0, length);
                stringToSplit = stringToSplit.Substring(length);
            }

            if (stringToSplit.Length > 0) yield return stringToSplit;
        }
    }
}