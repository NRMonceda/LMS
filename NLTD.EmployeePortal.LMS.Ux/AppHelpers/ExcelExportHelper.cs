using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

namespace NLTD.EmployeePortal.LMS.Ux.AppHelpers
{
    //http://www.c-sharpcorner.com/article/export-to-excel-in-asp-net-mvc/
    public class ExcelExportHelper
    {
        public static string ExcelContentType
        {
            get
            { return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
        }

        public static DataTable ListToDataTable<T>(List<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable dataTable = new DataTable();

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor property = properties[i];
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            object[] values = new object[properties.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = properties[i].GetValue(item);
                }

                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static byte[] ExportExcel(DataTable dataTable, string heading = "", bool showSrNo = false, params string[] columnsToTake)
        {
            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }

                dataTable.Columns["EmpID"].ColumnName = "Emp Id";
                dataTable.Columns["LeaveType"].ColumnName = "Request Type";
                dataTable.Columns["LeaveDate"].ColumnName = "Request Date";
                dataTable.Columns["PartOfDay"].ColumnName = "Part Of Day";
                dataTable.Columns["LeaveStatus"].ColumnName = "Status";
                dataTable.Columns["LeaveReason"].ColumnName = "Reason";
                dataTable.Columns["ApproverComments"].ColumnName = "Approver Comments";

                // add the content into the Excel file
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                // autofit width of cells with small content
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, 14];
                    //int maxLength = columnCells.Max(cell => cell.Value.ToString().Count()); //commented by suresh
                    //if (maxLength < 150)
                    //{
                    workSheet.Column(columnIndex).AutoFit();
                    //}

                    columnIndex++;
                }

                // format header - bold, yellow on black
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                // removed ignored columns
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }
                //custom code for this excel
                using (ExcelRange col = workSheet.Cells[2, 4, 2 + dataTable.Rows.Count, 4])
                {
                    col.Style.Numberformat.Format = "dd/MM/yyyy";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                //

                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;

                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }
        public static byte[] ExportExcelELCredit(DataTable dataTable, string heading = "", bool showSrNo = false, params string[] columnsToTake)
        {
            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }

                dataTable.Columns["EmployeeId"].ColumnName = "Emp Id";
                dataTable.Columns["Name"].ColumnName = "Name";
                dataTable.Columns["DOJ"].ColumnName = "Joining Date";
                dataTable.Columns["ConfirmationDate"].ColumnName = "Confirmation Date";
                dataTable.Columns["CurrentEL"].ColumnName = "Existing EL Balance";
                dataTable.Columns["ELCredit"].ColumnName = "EL Credit";
                dataTable.Columns["NewELBalance"].ColumnName = "Total EL Balance";

                // add the content into the Excel file
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                // autofit width of cells with small content
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, 14];
                    //int maxLength = columnCells.Max(cell => cell.Value.ToString().Count()); //commented by suresh
                    //if (maxLength < 150)
                    //{
                    workSheet.Column(columnIndex).AutoFit();
                    //}

                    columnIndex++;
                }

                // format header - bold, yellow on black
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                // removed ignored columns
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }
                //custom code for this excel
                using (ExcelRange col = workSheet.Cells[2, 3, 2 + dataTable.Rows.Count, 4])
                {
                    col.Style.Numberformat.Format = "dd/MM/yyyy";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                //

                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;

                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }
        public static byte[] ExportExcel<T>(List<T> data, string Heading = "", bool showSlno = false, params string[] ColumnsToTake)
        {
            return ExportExcel(ListToDataTable<T>(data), Heading, showSlno, ColumnsToTake);
        }

        public static byte[] ExportExcelELCredit<T>(List<T> data, string Heading = "", bool showSlno = false, params string[] ColumnsToTake)
        {
            return ExportExcelELCredit(ListToDataTable<T>(data), Heading, showSlno, ColumnsToTake);
        }



        public static byte[] ExportPermissionsExcel(DataTable dataTable, string heading = "", bool showSrNo = false, params string[] columnsToTake)
        {
            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }

                // add the content into the Excel file
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                // autofit width of cells with small content
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, 15];
                    //int maxLength = columnCells.Max(cell => cell.Value.ToString().Count()); //commented by suresh
                    //if (maxLength < 150)
                    //{
                    workSheet.Column(columnIndex).AutoFit();
                    //}
                    //workSheet.Column(9).Style.WrapText = true;
                    //workSheet.Column(10).Style.WrapText = true;
                    columnIndex++;
                }

                // format header - bold, yellow on black
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                // removed ignored columns
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }
                //custom code for this excel
                using (ExcelRange col = workSheet.Cells[2, 5, 2 + dataTable.Rows.Count, 5])
                {
                    col.Style.Numberformat.Format = "dd/MM/yyyy";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                //

                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;

                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }

        public static byte[] ExportOverTimeExcel(DataTable dataTable, string heading = "", bool showSrNo = false, params string[] columnsToTake)
        {
            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }

                dataTable.Columns["EmpID"].ColumnName = "Emp Id";
                dataTable.Columns["TimeFrom"].ColumnName = "Time From";
                dataTable.Columns["TimeTo"].ColumnName = "Time To";
                dataTable.Columns["PermissionDate"].ColumnName = "Over Time Date";
                dataTable.Columns["ApproverComments"].ColumnName = "Approver Comments";
                dataTable.Columns["ReportingManager"].ColumnName = "Reporting Manager";

                // add the content into the Excel file
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                // autofit width of cells with small content
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, 15];
                    //int maxLength = columnCells.Max(cell => cell.Value.ToString().Count()); //commented by suresh
                    //if (maxLength < 150)
                    //{
                    workSheet.Column(columnIndex).AutoFit();
                    //}
                    //workSheet.Column(9).Style.WrapText = true;
                    //workSheet.Column(10).Style.WrapText = true;
                    columnIndex++;
                }

                // format header - bold, yellow on black
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                // removed ignored columns
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }
                //custom code for this excel
                using (ExcelRange col = workSheet.Cells[2, 4, 2 + dataTable.Rows.Count, 4])
                {
                    col.Style.Numberformat.Format = "dd/MM/yyyy";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;

                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }

        public static byte[] ExportPermissionsExcel<T>(List<T> data, string Heading = "", bool showSlno = false, params string[] ColumnsToTake)
        {
            return ExportPermissionsExcel(ListToDataTable<T>(data), Heading, showSlno, ColumnsToTake);
        }

        public static byte[] ExportOverTimeExcel<T>(List<T> data, string Heading = "", bool showSlno = false, params string[] ColumnsToTake)
        {
            return ExportOverTimeExcel(ListToDataTable<T>(data), Heading, showSlno, ColumnsToTake);
        }

        public static byte[] ExportExcelYearSummary(DataTable dataTable, string heading = "", bool showSrNo = false, params string[] columnsToTake)
        {
            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }

                dataTable.Columns["EmpID"].ColumnName = "Emp Id";
                dataTable.Columns["LeaveType"].ColumnName = "Request Type";
                dataTable.Columns["TotalLeaves"].ColumnName = "Total Leaves";
                dataTable.Columns["UsedLeaves"].ColumnName = "Used Leaves";
                dataTable.Columns["PendingApproval"].ColumnName = "Pending Approval";
                dataTable.Columns["BalanceLeaves"].ColumnName = "Balance Leaves";
                // add the content into the Excel file
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                // autofit width of cells with small content
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    int maxLength = columnCells.Max(cell => cell.Value.ToString().Count());
                    if (maxLength < 150)
                    {
                        workSheet.Column(columnIndex).AutoFit();
                    }

                    columnIndex++;
                }

                // format header - bold, yellow on black
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                // removed ignored columns
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }

                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;

                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }

        public static byte[] ExportExcelYearSummary<T>(List<T> data, string Heading = "", bool showSlno = false, params string[] ColumnsToTake)
        {
            return ExportExcelYearSummary(ListToDataTable<T>(data), Heading, showSlno, ColumnsToTake);
        }

        public static byte[] ExportExcelMonthSummary(DataTable dataTable, int year, string heading = "", bool showSrNo = false, params string[] columnsToTake)
        {
            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                int startRowFrom = String.IsNullOrEmpty(heading) ? 3 : 5;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }

                workSheet.Cells["A1"].Value = "Year";
                workSheet.Cells["B1"].Value = year;
                // add the content into the Excel file
                workSheet.Cells["A2"].Value = "Emp Id";
                workSheet.Cells["B2"].Value = "Name";

                workSheet.Cells["C2"].Value = "CL/SL";
                workSheet.Cells["D2"].Value = "EL";
                workSheet.Cells["E2"].Value = "Debit";
                workSheet.Cells["F2"].Value = "LWP";
                workSheet.Cells["G2"].Value = "CompOff";

                workSheet.Cells["H2"].Value = "CL/SL";
                workSheet.Cells["I2"].Value = "EL";
                workSheet.Cells["J2"].Value = "Debit";
                workSheet.Cells["K2"].Value = "LWP";
                workSheet.Cells["L2"].Value = "CompOff";

                workSheet.Cells["M2"].Value = "CL/SL";
                workSheet.Cells["N2"].Value = "EL";
                workSheet.Cells["O2"].Value = "Debit";
                workSheet.Cells["P2"].Value = "LWP";
                workSheet.Cells["Q2"].Value = "CompOff";

                workSheet.Cells["R2"].Value = "CL/SL";
                workSheet.Cells["S2"].Value = "EL";
                workSheet.Cells["T2"].Value = "Debit";
                workSheet.Cells["U2"].Value = "LWP";
                workSheet.Cells["V2"].Value = "CompOff";

                workSheet.Cells["W2"].Value = "CL/SL";
                workSheet.Cells["X2"].Value = "EL";
                workSheet.Cells["Y2"].Value = "Debit";
                workSheet.Cells["Z2"].Value = "LWP";
                workSheet.Cells["AA2"].Value = "CompOff";

                workSheet.Cells["AB2"].Value = "CL/SL";
                workSheet.Cells["AC2"].Value = "EL";
                workSheet.Cells["AD2"].Value = "Debit";
                workSheet.Cells["AE2"].Value = "LWP";
                workSheet.Cells["AF2"].Value = "CompOff";

                workSheet.Cells["AG2"].Value = "CL/SL";
                workSheet.Cells["AH2"].Value = "EL";
                workSheet.Cells["AI2"].Value = "Debit";
                workSheet.Cells["AJ2"].Value = "LWP";
                workSheet.Cells["AK2"].Value = "CompOff";

                workSheet.Cells["AL2"].Value = "CL/SL";
                workSheet.Cells["AM2"].Value = "EL";
                workSheet.Cells["AN2"].Value = "Debit";
                workSheet.Cells["AO2"].Value = "LWP";
                workSheet.Cells["AP2"].Value = "CompOff";

                workSheet.Cells["AQ2"].Value = "CL/SL";
                workSheet.Cells["AR2"].Value = "EL";
                workSheet.Cells["AS2"].Value = "Debit";
                workSheet.Cells["AT2"].Value = "LWP";
                workSheet.Cells["AU2"].Value = "CompOff";

                workSheet.Cells["AV2"].Value = "CL/SL";
                workSheet.Cells["AW2"].Value = "EL";
                workSheet.Cells["AX2"].Value = "Debit";
                workSheet.Cells["AY2"].Value = "LWP";
                workSheet.Cells["AZ2"].Value = "CompOff";

                workSheet.Cells["BA2"].Value = "CL/SL";
                workSheet.Cells["BB2"].Value = "EL";
                workSheet.Cells["BC2"].Value = "Debit";
                workSheet.Cells["BD2"].Value = "LWP";
                workSheet.Cells["BE2"].Value = "CompOff";

                workSheet.Cells["BF2"].Value = "CL/SL";
                workSheet.Cells["BG2"].Value = "EL";
                workSheet.Cells["BH2"].Value = "Debit";
                workSheet.Cells["BI2"].Value = "LWP";
                workSheet.Cells["BJ2"].Value = "CompOff";

                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, false);

                // autofit width of cells with small content
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    int maxLength = columnCells.Max(cell => cell.Value.ToString().Count());
                    if (maxLength < 150)
                    {
                        workSheet.Column(columnIndex).AutoFit();
                    }

                    columnIndex++;
                }

                workSheet.Cells[1, 3, 1, 7].Merge = true;
                workSheet.Cells[1, 3, 1, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 3, 1, 7].Value = "January";
                workSheet.Cells[1, 3, 1, 7].Style.Font.Bold = true;
                workSheet.Cells[1, 3, 1, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 3, 1, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 8, 1, 12].Merge = true;
                workSheet.Cells[1, 8, 1, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 8, 1, 12].Value = "February";
                workSheet.Cells[1, 8, 1, 12].Style.Font.Bold = true;
                workSheet.Cells[1, 8, 1, 12].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 8, 1, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 13, 1, 17].Merge = true;
                workSheet.Cells[1, 13, 1, 17].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 13, 1, 17].Value = "March";
                workSheet.Cells[1, 13, 1, 17].Style.Font.Bold = true;
                workSheet.Cells[1, 13, 1, 17].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 13, 1, 17].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 18, 1, 22].Merge = true;
                workSheet.Cells[1, 18, 1, 22].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 18, 1, 22].Value = "April";
                workSheet.Cells[1, 18, 1, 22].Style.Font.Bold = true;
                workSheet.Cells[1, 18, 1, 22].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 18, 1, 22].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 23, 1, 27].Merge = true;
                workSheet.Cells[1, 23, 1, 27].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 23, 1, 27].Value = "May";
                workSheet.Cells[1, 23, 1, 27].Style.Font.Bold = true;
                workSheet.Cells[1, 23, 1, 27].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 23, 1, 27].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 28, 1, 32].Merge = true;
                workSheet.Cells[1, 28, 1, 32].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 28, 1, 32].Value = "June";
                workSheet.Cells[1, 28, 1, 32].Style.Font.Bold = true;
                workSheet.Cells[1, 28, 1, 32].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 28, 1, 32].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 33, 1, 37].Merge = true;
                workSheet.Cells[1, 33, 1, 37].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 33, 1, 37].Value = "July";
                workSheet.Cells[1, 33, 1, 37].Style.Font.Bold = true;
                workSheet.Cells[1, 33, 1, 37].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 33, 1, 37].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 38, 1, 42].Merge = true;
                workSheet.Cells[1, 38, 1, 42].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 38, 1, 42].Value = "August";
                workSheet.Cells[1, 38, 1, 42].Style.Font.Bold = true;
                workSheet.Cells[1, 38, 1, 42].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 38, 1, 42].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 43, 1, 47].Merge = true;
                workSheet.Cells[1, 43, 1, 47].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 43, 1, 47].Value = "September";
                workSheet.Cells[1, 43, 1, 47].Style.Font.Bold = true;
                workSheet.Cells[1, 43, 1, 47].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 43, 1, 47].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 48, 1, 52].Merge = true;
                workSheet.Cells[1, 48, 1, 52].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 48, 1, 52].Value = "October";
                workSheet.Cells[1, 48, 1, 52].Style.Font.Bold = true;
                workSheet.Cells[1, 48, 1, 52].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 48, 1, 52].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 53, 1, 57].Merge = true;
                workSheet.Cells[1, 53, 1, 57].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 53, 1, 57].Value = "November";
                workSheet.Cells[1, 53, 1, 57].Style.Font.Bold = true;
                workSheet.Cells[1, 53, 1, 57].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 53, 1, 57].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[1, 58, 1, 62].Merge = true;
                workSheet.Cells[1, 58, 1, 62].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 58, 1, 62].Value = "December";
                workSheet.Cells[1, 58, 1, 62].Style.Font.Bold = true;
                workSheet.Cells[1, 58, 1, 62].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[1, 58, 1, 62].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                // format header - bold, yellow on black
                using (ExcelRange r = workSheet.Cells[2, 1, 2, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders
                using (ExcelRange r = workSheet.Cells[1, 1, 1 + dataTable.Rows.Count + 1, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);

                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                using (ExcelRange r = workSheet.Cells[3, 2, 1 + dataTable.Rows.Count + 1, 2])
                {
                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                // removed ignored columns
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }

                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;

                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }

        public static byte[] ExportExcelMonthSummary<T>(List<T> data, int year, string Heading = "", bool showSlno = false, params string[] ColumnsToTake)
        {
            return ExportExcelMonthSummary(ListToDataTable<T>(data), year, Heading, showSlno, ColumnsToTake);
        }

        public static byte[] ExportExcelAttendance(List<EmployeeAttendanceModel> data, string Heading = "", bool showSlno = false, params string[] ColumnsToTake)
        {
            List<EmployeeAttendanceModel> employeeAttendanceModelListObj = (from at in data
                                                                            select new EmployeeAttendanceModel
                                                                            {
                                                                                UserID = at.UserID,
                                                                                InOut = at.InOut,
                                                                                AttendanceDate = at.InOutDate.ToString("dd-MM-yyyy"),
                                                                                InOutTime = at.InOutDate.ToString("HH:mm:ss"),
                                                                                Name = at.Name
                                                                            }).ToList();

            return ExportAttendanceExcel(employeeAttendanceModelListObj, Heading, showSlno, ColumnsToTake);
        }

        public static byte[] ExportExcelAccessCardAttendance(List<EmployeeAttendanceModel> data, string Heading = "", bool showSlno = false, params string[] ColumnsToTake)
        {
            List<EmployeeAttendanceModel> employeeAttendanceModelListObj = (from at in data
                                                                            select new EmployeeAttendanceModel
                                                                            {
                                                                                UserID = at.UserID,
                                                                                InOut = at.InOut,
                                                                                AttendanceDate = at.InOutDate.ToString("dd-MM-yyyy"),
                                                                                InOutTime = at.InOutDate.ToString("HH:mm:ss"),
                                                                                Name = at.Name,
                                                                                CardID = at.CardID
                                                                            }).ToList();

            return ExportAccessCardReportExcel(employeeAttendanceModelListObj, Heading, showSlno, ColumnsToTake);
        }

        public static byte[] ExportTimesheetExcel(List<TimeSheetModel> TimeSheetModelObj, string heading = "", bool showSrNo = false, params string[] columnsToTake)
        {
            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                DataTable dataTable = ListToDataTable<TimeSheetModel>(TimeSheetModelObj);
                dataTable.Columns["WorkingDate"].ColumnName = "Date";
                dataTable.Columns["ReportingManager"].ColumnName = "Reporting Manager";
                dataTable.Columns["InTime"].ColumnName = "In Time";
                dataTable.Columns["OutTime"].ColumnName = "Out Time";
                dataTable.Columns["WorkingHours"].ColumnName = "Working Hours";
                dataTable.Columns["LateIn"].ColumnName = "Late In";
                dataTable.Columns["EarlyOut"].ColumnName = "Early Out";
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add("Timesheet");
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;
                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    DataColumn dataColumn1 = dataTable.Columns.Add("Day", typeof(string));
                    dataColumn1.SetOrdinal(5);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        item["Day"] = Convert.ToDateTime(item["Date"]).DayOfWeek.ToString();
                        index++;
                    }
                }

                // add the content into the Excel file
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                // autofit width of cells with small content
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    workSheet.Column(columnIndex).AutoFit();
                    columnIndex++;
                }

                // format header - bold, yellow on black
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                //
                // removed ignored columns
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }

                //format the datetime field
                using (ExcelRange col = workSheet.Cells[2, 4, 1 + dataTable.Rows.Count, 4])
                {
                    col.Style.Numberformat.Format = "dd/MM/yyyy";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                using (ExcelRange col = workSheet.Cells[2, 6, 1 + dataTable.Rows.Count, 8])
                {
                    col.Style.Numberformat.Format = "HH:mm:ss";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                using (ExcelRange col = workSheet.Cells[2, 11, 1 + dataTable.Rows.Count, 12])
                {
                    col.Style.Numberformat.Format = "HH:mm:ss";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;

                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }
                //To Get the consolidate report Data
                List<ConsolidateReport> weeklyTimeSheetConsolidateList = new List<ConsolidateReport>();
                List<ConsolidateReport> monthlyTimeSheetConsolidateList = new List<ConsolidateReport>();
                GetWeeklyMonthlyReport(TimeSheetModelObj, out weeklyTimeSheetConsolidateList, out monthlyTimeSheetConsolidateList);

                // For Weekly Report
                dataTable = ListToDataTable<ConsolidateReport>(weeklyTimeSheetConsolidateList);
                dataTable.Columns["DateRange"].ColumnName = "Date";
                dataTable.Columns["TotalWorkingHours"].ColumnName = "Total Working Hours";
                dataTable.Columns["permissionCountOfficial"].ColumnName = "Total Permission Hours - Official";
                dataTable.Columns["permissionCountPersonal"].ColumnName = "Total Permission Hours - Personal";
                dataTable.Columns["LeaveCount"].ColumnName = "No .Of Leaves";
                dataTable.Columns["LateCount"].ColumnName = "No. Of Late In";
                dataTable.Columns["WorkFromHomeCount"].ColumnName = "No. Of Work From Home";
                dataTable.Columns["EarlyCount"].ColumnName = "No. Of Early out";
                dataTable.Columns["ReportingManager"].ColumnName = "Reporting Manager";
                workSheet = package.Workbook.Worksheets.Add("Summary");
                startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;
                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }

                // add the content into the Excel file
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);
                workSheet = FormatConsolidateExcel(workSheet, dataTable, startRowFrom, showSrNo);
                // autofit width of cells with small content

                //dataTable = ListToDataTable<ConsolidateReport>(monthlyTimeSheetConsolidateList);
                //workSheet = package.Workbook.Worksheets.Add("Monthly Report");
                //startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;
                //if (showSrNo)
                //{
                //    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                //    dataColumn.SetOrdinal(0);
                //    int index = 1;
                //    foreach (DataRow item in dataTable.Rows)
                //    {
                //        item[0] = index;
                //        index++;
                //    }
                //}

                //// add the content into the Excel file
                //workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);
                //// autofit width of cells with small content
                //workSheet = FormatConsolidateExcel(workSheet, dataTable, startRowFrom, showSrNo);
                result = package.GetAsByteArray();
            }

            return result;
        }

        public static void GetWeeklyMonthlyReport(List<TimeSheetModel> timeSheetModelList, out List<ConsolidateReport> weeklyTimeSheetConsolidateList, out List<ConsolidateReport> monthlyTimeSheetConsolidateList)
        {
            weeklyTimeSheetConsolidateList = new List<ConsolidateReport>();
            monthlyTimeSheetConsolidateList = new List<ConsolidateReport>();
            List<Int64> userIDList = (from e in timeSheetModelList select e.userID).Distinct().ToList();
            for (int userIDIndex = 0; userIDIndex < userIDList.Count; userIDIndex++)
            {
                List<TimeSheetModel> timeSheet = (from t in timeSheetModelList where t.userID == userIDList[userIDIndex] select t).ToList();
                ConsolidateReport weeklyConsolidateReport = null;
                ConsolidateReport monthlyConsolidateReport = null;
                DateTime weeklyFromDate = new DateTime();
                DateTime monthlyFromDate = new DateTime();
                for (int i = 0; i < timeSheet.Count(); i++)
                {
                    if (weeklyConsolidateReport == null)
                    {
                        weeklyFromDate = timeSheet[i].WorkingDate;
                        monthlyFromDate = timeSheet[i].WorkingDate;
                        weeklyConsolidateReport = new ConsolidateReport();
                        monthlyConsolidateReport = new ConsolidateReport();
                    }
                    weeklyConsolidateReport = CalculateTimeSheetConsolidation(timeSheet[i], weeklyConsolidateReport);
                    monthlyConsolidateReport = CalculateTimeSheetConsolidation(timeSheet[i], monthlyConsolidateReport);
                    if (timeSheet[i].WorkingDate.ToString("ddd") == "Mon")
                    {
                        weeklyConsolidateReport.DateRange = string.Format("{0} to {1}", timeSheet[i].WorkingDate.ToString("dd/MM/yyyy"), weeklyFromDate.ToString("dd/MM/yyyy"));
                        weeklyConsolidateReport.Name = timeSheet[i].Name;
                        weeklyConsolidateReport.ReportingManager = timeSheet[i].ReportingManager;
                        weeklyConsolidateReport.TotalWorkingHours = GetWorkingHours(weeklyConsolidateReport.WorkingHours);
                        weeklyTimeSheetConsolidateList.Add(weeklyConsolidateReport);
                        weeklyConsolidateReport = new ConsolidateReport();
                        weeklyFromDate = timeSheet[i].WorkingDate.AddDays(-1);
                    }
                    if (timeSheet[i].WorkingDate.Day == 1)
                    {
                        monthlyConsolidateReport.DateRange = string.Format("{0} to {1}", timeSheet[i].WorkingDate.ToString("dd/MM/yyyy"), monthlyFromDate.ToString("dd/MM/yyyy"));
                        monthlyConsolidateReport.Name = timeSheet[i].Name;
                        monthlyConsolidateReport.ReportingManager = timeSheet[i].ReportingManager;
                        monthlyConsolidateReport.TotalWorkingHours = GetWorkingHours(monthlyConsolidateReport.WorkingHours);
                        monthlyTimeSheetConsolidateList.Add(monthlyConsolidateReport);
                        monthlyConsolidateReport = new ConsolidateReport();
                        monthlyFromDate = timeSheet[i].WorkingDate.AddDays(-1);
                    }
                    if (i == timeSheet.Count - 1 && timeSheet[i].WorkingDate.ToString("ddd") != "Mon")
                    {
                        weeklyConsolidateReport.DateRange = string.Format("{0} to {1}", timeSheet[i].WorkingDate.ToString("dd/MM/yyyy"), weeklyFromDate.ToString("dd/MM/yyyy"));
                        weeklyConsolidateReport.Name = timeSheet[i].Name;
                        weeklyConsolidateReport.ReportingManager = timeSheet[i].ReportingManager;
                        weeklyConsolidateReport.TotalWorkingHours = GetWorkingHours(weeklyConsolidateReport.WorkingHours);
                        weeklyTimeSheetConsolidateList.Add(weeklyConsolidateReport);
                    }
                    if (i == timeSheet.Count - 1 && timeSheet[i].WorkingDate.Day != 1)
                    {
                        monthlyConsolidateReport.DateRange = string.Format("{0} to {1}", timeSheet[i].WorkingDate.ToString("dd/MM/yyyy"), monthlyFromDate.ToString("dd/MM/yyyy"));
                        monthlyConsolidateReport.Name = timeSheet[i].Name;
                        monthlyConsolidateReport.ReportingManager = timeSheet[i].ReportingManager;
                        monthlyConsolidateReport.TotalWorkingHours = GetWorkingHours(monthlyConsolidateReport.WorkingHours);
                        monthlyTimeSheetConsolidateList.Add(monthlyConsolidateReport);
                    }
                }
            }
        }

        public static ExcelWorksheet FormatConsolidateExcel(ExcelWorksheet workSheet, DataTable dataTable, int startRowFrom, bool showSrNo)
        {
            // removed ignored columns

            int columnIndex = 1;
            foreach (DataColumn column in dataTable.Columns)
            {
                ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                workSheet.Column(columnIndex).AutoFit();
                columnIndex++;
            }

            // format header - bold, yellow on black
            using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
            {
                r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                r.Style.Font.Bold = true;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
            }

            // format cells - add borders
            using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
            {
                r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
            }

            // Formating Working Hours
            using (ExcelRange col = workSheet.Cells[2, 4, 1 + dataTable.Rows.Count, 4])
            {
                col.Style.Numberformat.Format = "HH:mm:ss";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
            string[] columnsToRemove = { "WorkingHours", "FromDate" };
            for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
            {
                if (i == 0 && showSrNo)
                {
                    continue;
                }
                if (columnsToRemove.Contains(dataTable.Columns[i].ColumnName))
                {
                    workSheet.DeleteColumn(i + 1);
                }
            }
            return workSheet;
            //For Monthly report
        }

        public static ConsolidateReport CalculateTimeSheetConsolidation(TimeSheetModel TimeSheetModelObj, ConsolidateReport TimeSheetConsolidateObj)
        {
            string personalPermisionLabel = ConfigurationManager.AppSettings["PersonalPermission"].ToString();
            string officialPermisionLabel = ConfigurationManager.AppSettings["OfficialPermission"].ToString();

            TimeSheetConsolidateObj.WorkingHours = TimeSheetConsolidateObj.WorkingHours + TimeSheetModelObj.WorkingHours;
            if (TimeSheetModelObj.LateIn.Seconds > 0)
            {
                TimeSheetConsolidateObj.LateCount = TimeSheetConsolidateObj.LateCount + 1;
            }
            if (TimeSheetModelObj.EarlyOut.Seconds > 0)
            {
                TimeSheetConsolidateObj.EarlyCount = TimeSheetConsolidateObj.EarlyCount + 1;
            }
            if (!string.IsNullOrEmpty(TimeSheetModelObj.Requests))
            {
                if (TimeSheetModelObj.permissionCountOfficial > 0)
                {
                    TimeSheetConsolidateObj.permissionCountOfficial = TimeSheetConsolidateObj.permissionCountOfficial + TimeSheetModelObj.permissionCountOfficial;
                }
                if (TimeSheetModelObj.permissionCountPersonal > 0)
                {
                    TimeSheetConsolidateObj.permissionCountPersonal = TimeSheetConsolidateObj.permissionCountPersonal + TimeSheetModelObj.permissionCountPersonal;
                }
                if (TimeSheetModelObj.Requests.Contains("Leave"))
                {
                    TimeSheetConsolidateObj.LeaveCount = TimeSheetConsolidateObj.LeaveCount + TimeSheetModelObj.LeaveDayQty;
                }
                if (TimeSheetModelObj.Requests.Contains("Work From Home"))
                {
                    TimeSheetConsolidateObj.WorkFromHomeCount = TimeSheetConsolidateObj.WorkFromHomeCount + TimeSheetModelObj.WorkFromHomeDayQty;
                }
            }
            return TimeSheetConsolidateObj;
        }

        public static string GetWorkingHours(TimeSpan t)
        {
            string totalHours = Math.Floor(t.TotalHours).ToString();
            if (totalHours.Length == 1)
            {
                totalHours = "0" + totalHours;
            }
            return string.Format("{0}:{1}", totalHours, t.ToString("mm\\:ss"));
        }

        public static byte[] ExportAccessCardReportExcel(List<EmployeeAttendanceModel> data, string heading = "", bool showSrNo = false, params string[] columnsToTake)
        {
            byte[] result = null;

            using (ExcelPackage package = new ExcelPackage())
            {
                DataTable dataTable = ListToDataTable<EmployeeAttendanceModel>(data);

                dataTable.Columns["CardID"].ColumnName = "Card Id";
                dataTable.Columns["AttendanceDate"].ColumnName = "Date";
                dataTable.Columns["InOutTime"].ColumnName = "In Out Time";
                dataTable.Columns["InOut"].ColumnName = "In Out";

                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add("AttendanceDetails");
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);

                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;

                        index++;
                    }
                }

                // add the content into the Excel file
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                // autofit width of cells with small content
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    //int maxLength = columnCells.Max(cell => cell.Value.ToString().Count());
                    //if (maxLength < 150)
                    //{
                    workSheet.Column(columnIndex).AutoFit();
                    //}

                    columnIndex++;
                }

                // format header - bold, yellow on black
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                //
                // removed ignored columns
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }

                //format the datetime field
                //using (ExcelRange col = workSheet.Cells[2, 3, 1 + dataTable.Rows.Count, 3])
                //{
                //    col.Style.Numberformat.Format = "dd/MM/yyyy";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}
                //using (ExcelRange col = workSheet.Cells[2, 5, 1 + dataTable.Rows.Count, 7])
                //{
                //    col.Style.Numberformat.Format = "HH:mm:ss";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}
                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;

                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }
                result = package.GetAsByteArray();
            }
            return result;
        }
        public static byte[] ExportAttendanceExcel(List<EmployeeAttendanceModel> data, string heading = "", bool showSrNo = false, params string[] columnsToTake)
        {
            byte[] result = null;

            using (ExcelPackage package = new ExcelPackage())
            {
                DataTable dataTable = ListToDataTable<EmployeeAttendanceModel>(data);
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add("AttendanceDetails");
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);

                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;

                        index++;
                    }
                }

                // add the content into the Excel file
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                // autofit width of cells with small content
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    //int maxLength = columnCells.Max(cell => cell.Value.ToString().Count());
                    //if (maxLength < 150)
                    //{
                    workSheet.Column(columnIndex).AutoFit();
                    //}

                    columnIndex++;
                }

                // format header - bold, yellow on black
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                //
                // removed ignored columns
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }

                //format the datetime field
                //using (ExcelRange col = workSheet.Cells[2, 3, 1 + dataTable.Rows.Count, 3])
                //{
                //    col.Style.Numberformat.Format = "dd/MM/yyyy";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}
                //using (ExcelRange col = workSheet.Cells[2, 5, 1 + dataTable.Rows.Count, 7])
                //{
                //    col.Style.Numberformat.Format = "HH:mm:ss";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}
                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;

                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }
                result = package.GetAsByteArray();
            }
            return result;
        }
    }

    public class ConsolidateReport
    {
        public string Name { get; set; }
        public string DateRange { get; set; }
        public TimeSpan WorkingHours { get; set; }

        public string TotalWorkingHours { get; set; }

        public decimal LeaveCount { get; set; }

        public decimal WorkFromHomeCount { get; set; }
        public decimal permissionCountOfficial { get; set; }
        public decimal permissionCountPersonal { get; set; }
        public int LateCount { get; set; }
        public int EarlyCount { get; set; }

        public DateTime FromDate { get; set; }

        public string ReportingManager { get; set; }
    }
}