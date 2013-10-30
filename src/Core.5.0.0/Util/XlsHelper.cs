using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Web;
using com.Sconit.Entity;
using NPOI.SS.UserModel;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace com.Sconit.Utility
{
    public class XlsHelper
    {
        /**
         * UTF8编码文件名
         * 
         * Param fileName 文件名
         * 
         * Return 文件名
         */
        public static string UTF_FileName(string filename)
        {
            return HttpUtility.UrlEncode(filename, System.Text.Encoding.UTF8);
        }



        /*
        * 响应到客户端
        *
        * Param fileName 文件名
        */
        public static void WriteToClient(String fileName, HSSFWorkbook workbook)
        {
            //Write the stream data of workbook to the root directory
            //FileStream file = new FileStream(@"c:/test.xls", FileMode.Create);
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ClearHeaders();

            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.Expires = 0;
            HttpContext.Current.Response.CacheControl = "no-cache";

            HttpContext.Current.Response.ContentType = "application/x-excel";
            //inline,attachment
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + UTF_FileName(fileName));
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
            workbook.Write(HttpContext.Current.Response.OutputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            sheet = null;
            workbook.Dispose();
            workbook = null;

            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
            //file.Close();
        }
        /*
         * 生成文件
         *
         * Return 生成文件的URL
         */
        public static string WriteToFile(HSSFWorkbook workbook)
        {
            return WriteToFile("temp.xls", workbook);
        }

        /*
         * 生成文件
         *
         * Param fileName 文件名
         * 
         * Return 生成文件的URL
         */
        public static string WriteToFile(String fileName, HSSFWorkbook workbook)
        {

            //临时文件路径
            string tempFilePath;
            if (HttpContext.Current == null)
            {
                tempFilePath = System.Environment.GetEnvironmentVariable("TEMP") + "\\";
            }
            else
            {
                tempFilePath = HttpContext.Current.Server.MapPath("~/" + BusinessConstants.TEMP_FILE_PATH);
            }
            string tempFileName = GetRandomFileName(fileName);

            if (!Directory.Exists(tempFilePath))
                Directory.CreateDirectory(tempFilePath);

            //Write the stream data of workbook to the root directory
            FileStream file = new FileStream(tempFilePath + tempFileName, FileMode.Create);
            workbook.Write(file);
            file.Flush();
            file.Close();
            file.Dispose();
            file = null;

            ISheet sheet = workbook.GetSheetAt(0);
            sheet = null;
            workbook.Dispose();
            workbook = null;

            if (HttpContext.Current == null)
            {
                return tempFilePath + tempFileName;
            }
            else
            {
                return GetShowFileUrl(tempFileName);
            }
        }


        /**
        * 生成随即文件名
        * 
        * Param tempFileName 模版文件名
        * 
        * Return 随即文件名
        */
        private static string GetRandomFileName(string tempFileName)
        {
            string templateFileName = tempFileName.Substring(0, tempFileName.LastIndexOf("."));
            string extension = tempFileName.Substring(tempFileName.LastIndexOf(".") + 1);

            string fileName = templateFileName + "_" + DateTime.Now.ToString("yyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N");
            if (extension != null && extension.Trim() != string.Empty)
                fileName += "." + extension;

            return fileName;
        }


        /**
        * 获得报表URL
        * 
        * Param fileName 文件名
        * 
        * Return 报表URL
        */
        private static string GetShowFileUrl(string fileName)
        {
            //string url = HttpContext.Current.Request.Url.AbsoluteUri;
            //string showFileURL = string.Empty;
            //int index = 0;
            //if (url.EndsWith(".aspx"))
            //{
            //    index = url.IndexOf(".aspx");
            //    if (index > 0)
            //    {
            //        url = url.Remove(index);
            //        index = url.LastIndexOf("/");
            //        showFileURL = url.Remove(index + 1) + BusinessConstants.TEMP_FILE_PATH + fileName;
            //    }
            //}
            //else if (url.EndsWith(".asmx"))//支持webservice
            //{
            //    index = url.IndexOf(".asmx");
            //}

            string url = "http://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath + "/";
            url = url + BusinessConstants.TEMP_FILE_PATH + fileName;
            return url;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="gv"></param>
        public static void Export(string fileName, GridView gv)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader(
                "content-disposition", string.Format("attachment; filename={0}", fileName));
            HttpContext.Current.Response.ContentType = "application/ms-excel";

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    //  Create a table to contain the grid
                    Table table = new Table();

                    //  include the gridline settings
                    table.GridLines = gv.GridLines;

                    //  add the header row to the table
                    if (gv.HeaderRow != null)
                    {
                        PrepareControlForExport(gv.HeaderRow);
                        table.Rows.Add(gv.HeaderRow);
                    }

                    //  add each of the data rows to the table
                    foreach (GridViewRow row in gv.Rows)
                    {
                        PrepareControlForExport(row);
                        table.Rows.Add(row);
                    }

                    //  add the footer row to the table
                    if (gv.FooterRow != null)
                    {
                        PrepareControlForExport(gv.FooterRow);
                        table.Rows.Add(gv.FooterRow);
                    }

                    //  render the table into the htmlwriter
                    table.RenderControl(htw);

                    //  render the htmlwriter into the response
                    HttpContext.Current.Response.Write(sw.ToString());
                    HttpContext.Current.Response.End();
                }
            }
        }

        /// <summary>
        /// Replace any of the contained controls with literals
        /// </summary>
        /// <param name="control"></param>
        private static void PrepareControlForExport(Control control)
        {
            for (int i = 0; i < control.Controls.Count; i++)
            {
                Control current = control.Controls[i];
                if (current is LinkButton)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as LinkButton).Text));
                }
                else if (current is ImageButton)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as ImageButton).AlternateText));
                }
                else if (current is HyperLink)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as HyperLink).Text));
                }
                else if (current is DropDownList)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as DropDownList).SelectedItem.Text));
                }
                else if (current is CheckBox)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as CheckBox).Checked ? "True" : "False"));
                }

                if (current.HasControls())
                {
                    PrepareControlForExport(current);
                }
            }
        }

        public static void SetRowCell(ISheet sheet, int rownum, int colnum, string value)
        {
            SetRowCell(sheet, rownum, colnum, value, null);
        }

        public static void SetRowCell(ISheet sheet, int rownum, int colnum, string value, ICellStyle cellStyle)
        {
            IRow row = GetRow(sheet, rownum);
            ICell cell = row.GetCell(colnum);
            if (cell == null) cell = row.CreateCell(colnum);
            cell.SetCellValue(value);
            if (cellStyle != null)
            {
                cell.CellStyle = cellStyle;
            }
        }

        /**
         * 
         * 获得行对象
         * 
         * Param rowIndex 行号
         * Return 行对象
         */
        public static IRow GetRow(ISheet sheet, int rownum)
        {
            IRow row = null;
            row = sheet.GetRow(rownum);
            if (row == null) row = sheet.CreateRow(rownum);
            return row;
        }



        public static void WriteToFile(string folder, String fileName, IWorkbook workbook)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            FileStream file = new FileStream(folder + "/" + fileName, FileMode.Create);
            workbook.Write(file);
            file.Close();
        }
    }
}
