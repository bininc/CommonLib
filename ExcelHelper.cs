using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using CommLiby;
using NPOI.HPSF;
using NPOI.HSSF.EventUserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace CommonLib
{
    /// <summary>
    /// Excel 助手类。
    /// </summary>
    public static class ExcelHelper
    {

        public static HSSFWorkbook CreateWorkbook()
        {
            HSSFWorkbook hssfworkbook = new HSSFWorkbook();
            InitializeWorkbook(hssfworkbook, "导出的Excel");
            return hssfworkbook;
        }

        public static void InitializeWorkbook(HSSFWorkbook hssfworkbook, string title)
        {
            hssfworkbook = new HSSFWorkbook();

            //创建一个文档摘要信息实体。
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = ""; //公司名称
            hssfworkbook.DocumentSummaryInformation = dsi;

            //创建一个摘要信息实体。
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "系统生成";
            si.Author = "系统";
            si.Title = title;
            si.CreateDateTime = DateTimeHelper.Now;
            hssfworkbook.SummaryInformation = si;
        }

        /// <summary>
        /// 将工作表写入到内存流中
        /// </summary>
        /// <param name="hssfworkbook"></param>
        /// <returns></returns>
        public static MemoryStream WriteToStream(HSSFWorkbook hssfworkbook)
        {
            //Write the stream data of workbook to the root directory
            MemoryStream file = new MemoryStream();
            hssfworkbook.Write(file);
            return file;
        }
        /// <summary>
        /// 将工作写入Bytes数组
        /// </summary>
        /// <param name="hssfWorkbook"></param>
        /// <returns></returns>
        public static byte[] WriteToBytes(HSSFWorkbook hssfWorkbook)
        {
            MemoryStream ms = WriteToStream(hssfWorkbook);
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }

        //Export(DataTable table, string headerText, string sheetName, string[] columnName, string[] columnTitle)
        /// <summary>
        /// 向客户端输出文件。
        /// </summary>
        /// <param name="table">数据表。</param>
        /// <param name="headerText">头部文本。</param>
        /// <param name="sheetName"></param>
        /// <param name="columnName">数据列名称。</param>
        /// <param name="columnTitle">表标题。</param>
        /// <param name="fileName">文件名称。</param>
        public static void Write(DataTable table, string headerText, string sheetName, SheetColumn[] sheetColumns, string fileName)
        {
            HSSFWorkbook hssfworkbook = GenerateData(table, headerText, sheetName, sheetColumns);
            MemoryStream ms = WriteToStream(hssfworkbook);
            string encodeFileName = HttpUtility.UrlEncode(fileName);
            if (encodeFileName.Length > 215)
                encodeFileName = encodeFileName.Substring(0, 210);

            HttpContext context = HttpContext.Current;
            context.Response.ClearContent();
            context.Response.Clear();
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.AddHeader("Content-Length", ms.Length.ToString());
            context.Response.ContentType = "application/octet-stream";
            context.Response.AddHeader("Content-Disposition", string.Format("inline;filename={0}.xls", encodeFileName));
            int readLength = 0;
            byte[] buffer = new byte[1024];
            ms.Position = 0;
            while (true)
            {
                if (context.Response.IsClientConnected)
                {
                    readLength = ms.Read(buffer, 0, buffer.Length);
                    if (readLength == 0) break;
                    context.Response.OutputStream.Write(buffer, 0, readLength);
                    context.Response.Flush();
                }
                else
                {
                    break;
                }
            }
            context.Response.Close();
            ms.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="headerText"></param>
        /// <param name="sheetName"></param>
        /// <param name="columnName"></param>
        /// <param name="columnTitle"></param>
        /// <returns></returns>
        public static HSSFWorkbook GenerateData(DataTable table, string headerText, string sheetName, SheetColumn[] sheetColumns)
        {
            HSSFWorkbook hssfworkbook = CreateWorkbook();
            ISheet sheet = hssfworkbook.CreateSheet(sheetName);

            #region 日期格式
            ICellStyle dateStyle = hssfworkbook.CreateCellStyle();
            IDataFormat format = hssfworkbook.CreateDataFormat();
            dateStyle.DataFormat = format.GetFormat("yyyy-MM-dd HH:mm:ss");
            #endregion

            #region 新建表，填充表头，填充列头，样式

            #region 取得列宽
            int[] colWidth = new int[sheetColumns.Length];

            for (int i = 0; i < sheetColumns.Length; i++)
            {
                colWidth[i] = Encoding.GetEncoding(936).GetBytes(sheetColumns[i].Header).Length;
            }
            for (int i = 0; i < table.Rows.Count; i++)
            {
                for (int j = 0; j < sheetColumns.Length; j++)
                {
                    int intTemp = Encoding.GetEncoding(936).GetBytes(table.Rows[i][sheetColumns[j].DataProperty].ToString()).Length;
                    if (intTemp > colWidth[j])
                    {
                        colWidth[j] = intTemp;
                    }
                }
            }
            #endregion

            #region 表头及样式
            //if (!string.IsNullOrEmpty(headerText))
            {
                IRow headerRow = sheet.CreateRow(0);
                headerRow.HeightInPoints = 25;
                headerRow.CreateCell(0).SetCellValue(headerText);

                ICellStyle headStyle = hssfworkbook.CreateCellStyle();
                headStyle.Alignment = HorizontalAlignment.Center;
                IFont font = hssfworkbook.CreateFont();
                font.FontHeightInPoints = 20;
                font.Boldweight = 700;
                headStyle.SetFont(font);

                headerRow.GetCell(0).CellStyle = headStyle;
                //sheet.AddMergedRegion(new Region(0, 0, 0, dtSource.Columns.Count - 1)); 
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, sheetColumns.Length - 1));
            }
            #endregion

            #region 列头及样式
            {
                IRow headerRow = sheet.CreateRow(1);
                ICellStyle headStyle = hssfworkbook.CreateCellStyle();
                headStyle.Alignment = HorizontalAlignment.Center;
                IFont font = hssfworkbook.CreateFont();
                font.FontHeightInPoints = 10;
                font.Boldweight = 700;
                headStyle.SetFont(font);

                for (int i = 0; i < sheetColumns.Length; i++)
                {
                    headerRow.CreateCell(i).SetCellValue(sheetColumns[i].Header);
                    headerRow.GetCell(i).CellStyle = headStyle;
                    //设置列宽 
                    if ((colWidth[i] + 1) * 256 > 30000)
                    {
                        sheet.SetColumnWidth(i, 10000);
                    }
                    else
                    {
                        sheet.SetColumnWidth(i, (colWidth[i] + 1) * 256);
                    }
                }
                /* 
                foreach (DataColumn column in dtSource.Columns) 
                { 
                    headerRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName); 
                    headerRow.GetCell(column.Ordinal).CellStyle = headStyle; 
   
                    //设置列宽    
                    sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256); 
                } 
                 * */
            }
            #endregion

            #endregion

            #region 填充数据
            int rowIndex = 2;
            foreach (DataRow row in table.Rows)
            {
                //if (rowIndex == 65535)
                //{
                //    sheet = hssfworkbook.CreateSheet(sheetName + ((int)rowIndex / 65535).ToString());
                //}
                #region 填充数据

                IRow dataRow = sheet.CreateRow(rowIndex);
                for (int i = 0; i < sheetColumns.Length; i++)
                {
                    ICell newCell = dataRow.CreateCell(i);

                    object drValue = row[sheetColumns[i].DataProperty];
                    drValue = Common.Convert2Type(table.Columns[sheetColumns[i].DataProperty].DataType, drValue);
                    //newCell.SetCellValue(value);
                    if (drValue != DBNull.Value)
                    {
                        switch (table.Columns[sheetColumns[i].DataProperty].DataType.ToString())
                        {
                            case "System.String": //字符串类型   
                                newCell.SetCellValue((string)drValue);
                                break;
                            case "System.DateTime": //日期类型    
                                newCell.SetCellValue(Convert.ToDateTime(drValue));

                                newCell.CellStyle = dateStyle; //格式化显示    
                                break;
                            case "System.Boolean": //布尔型    
                                bool boolV = (bool)drValue;
                                if (boolV)
                                    newCell.SetCellValue("是");
                                else
                                    newCell.SetCellValue("否");
                                break;
                            case "System.Int16": //整型    
                            case "System.Int32":
                            case "System.Int64":
                            case "System.Byte":
                                int intV = Convert.ToInt32(drValue);
                                newCell.SetCellValue(intV);
                                break;
                            case "System.Decimal": //浮点型    
                            case "System.Double":
                                double doubV = Convert.ToDouble(drValue);
                                newCell.SetCellValue(doubV);
                                break;
                            case "System.DBNull": //空值处理    
                                newCell.SetCellValue("");
                                break;
                            default:
                                newCell.SetCellValue("");
                                break;
                        }
                    }
                }

                #endregion

                rowIndex++;
            }
            #endregion

            return hssfworkbook;
        }

        /// <summary>
        /// 读excel内容到list列表里
        /// </summary>
        /// <param name="buf">excel文件的字节数组形式</param>
        /// <returns>List<List<string>></returns>
        public static List<List<string>> ReadExcel(byte[] buf)
        {
            List<List<string>> r = new List<List<string>>();
            IWorkbook workbook = new HSSFWorkbook(new MemoryStream(buf));
            ISheet sheet = workbook.GetSheetAt(0);
            int rowCount = sheet.LastRowNum;
            for (int i = 0; i <= rowCount; i++)
            {
                List<string> list = new List<string>();
                r.Add(list);
                IRow row = sheet.GetRow(i);
                for (int j = 0; j < row.Cells.Count; j++)
                {
                    list.Add(row.GetCell(j).ToString());
                }
            }
            return r;
        }

        public static byte[] FillData(byte[] excelBytes, string sheetName, List<string[]> data, int startRow = 1, Dictionary<ExcelPosition, string> otherData = null)
        {
            if (data == null || excelBytes == null) return null;
            HSSFWorkbook workbook = new HSSFWorkbook(new MemoryStream(excelBytes));
            ISheet sheet = string.IsNullOrWhiteSpace(sheetName) ? workbook.GetSheetAt(0) : workbook.GetSheet(sheetName);
            while (workbook.NumberOfSheets != 1)
            {
                ISheet tmp = workbook.GetSheetAt(0);
                if (tmp.SheetName != sheet.SheetName)
                    workbook.RemoveSheetAt(0);
                else
                    workbook.RemoveSheetAt(1);
            }

            for (var i = 0; i < data.Count; i++)
            {
                IRow row = sheet.GetRow(i + startRow - 1) ?? sheet.CreateRow(i + startRow - 1);

                string[] rowdata = data[i];
                for (var j = 0; j < rowdata.Length; j++)
                {
                    string val = rowdata[j];
                    if (val == null) continue;

                    ICell cell = row.GetCell(j) ?? row.CreateCell(j);
                    cell.SetCellValue(val);
                }
            }

            if (otherData != null)
                foreach (var item in otherData)
                {
                    ExcelPosition pos = item.Key;
                    string val = item.Value;
                    IRow row = sheet.GetRow(pos.RowIndex) ?? sheet.CreateRow(pos.RowIndex);
                    ICell cell = row.GetCell(pos.CellIndex) ?? row.CreateCell(pos.CellIndex);
                    cell.SetCellValue(val);
                }

            return WriteToBytes(workbook);
        }

        public static byte[] FillData(DataTable table, string sheetName, SheetColumn[] sheetColumns = null)
        {
            HSSFWorkbook hssfworkbook = CreateWorkbook();
            ISheet sheet = hssfworkbook.CreateSheet(sheetName);

            int startRow = 1;
            int colCount = 0;
            bool hasHeader = false;
            if (sheetColumns != null && sheetColumns.Length > 1)
            {
                IRow titleRow = sheet.GetRow(0) ?? sheet.CreateRow(0);
                foreach (SheetColumn col in sheetColumns)
                {
                    ICell cell = titleRow.GetCell(col.ColIndex) ?? titleRow.CreateCell(col.ColIndex);
                    cell.SetCellValue(col.Header);
                }
                startRow += 1;
                colCount = sheetColumns.Length;
                hasHeader = true;
            }

            if (table.IsNotEmpty())
            {
                if (colCount == 0) colCount = table.Columns.Count;
                for (var i = 0; i < table.Rows.Count; i++)
                {
                    IRow row = sheet.GetRow(i + startRow - 1) ?? sheet.CreateRow(i + startRow - 1);

                    DataRow rowdata = table.Rows[i];
                    for (var j = 0; j < colCount; j++)
                    {
                        string val = null;
                        if (hasHeader)
                        {
                            val = rowdata.GetDataRowStringValue(sheetColumns.First(c => c.ColIndex == j).DataProperty);
                        }
                        else
                        {
                            val = rowdata.GetDataRowStringValue(j);
                        }
                        if (val == null) continue;

                        ICell cell = row.GetCell(j) ?? row.CreateCell(j);
                        cell.SetCellValue(val);
                    }
                }
            }

            return WriteToBytes(hssfworkbook);
        }
    }

    /// <summary>
    /// 表格位置
    /// </summary>
    public class ExcelPosition
    {
        /// <summary>
        /// 行位置
        /// </summary>
        public int RowIndex { get; set; }
        /// <summary>
        /// 列位置
        /// </summary>
        public int CellIndex { get; set; }
    }
}

