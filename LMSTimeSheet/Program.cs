﻿using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;

namespace LMSTimeSheet
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                // Connection string
                var lmsConnection = ConfigurationManager.ConnectionStrings["LMSConnection"].ConnectionString;
                string msAccessConnection = ConfigurationManager.ConnectionStrings["MSAccessConnection"].ConnectionString;

                WriteLog("Started Processing Timesheet data");

                //Get lastUpdated transaction for processing
                int lastUpdatedTransaction = CheckTimeSheetLastTransaction(lmsConnection);
                if (lastUpdatedTransaction == 0)
                {
                    lastUpdatedTransaction = Convert.ToInt32(ConfigurationManager.AppSettings["FetchTransactionFrom"]);
                }

                //Get the access Details
                var accessResults = GetMsAccessDetails(msAccessConnection, lastUpdatedTransaction);

                //Adding new TimeSheet Details
                if (accessResults.Rows.Count > 0)
                {
                    WriteLog("Adding MS Access " + accessResults.Rows.Count + " rows To LMS Database");

                    BulkInsertToTimeSheet(lmsConnection, accessResults);

                    WriteLog("Processed MS Access " + accessResults.Rows.Count + " rows To LMS Database");
                }
                else
                {
                    WriteLog("No MS Access Record for Processing Timesheet");
                }

                WriteLog("Mapping Employee with Timesheet");

                MapEmployeeWithTimesheet(lmsConnection);

                WriteLog("Processed Timesheet " + accessResults.Rows.Count + " rows");

                accessResults.Clear();
            }
            catch (Exception e)
            {
                WriteLog("Process failed due to " + e.Message);
            }
        }

        private static int CheckTimeSheetLastTransaction(string lmsConnection)
        {
            int lastUpdatedTransaction;
            SqlConnection conn = new SqlConnection(lmsConnection);

            conn.Open();

            SqlCommand comm = new SqlCommand("SELECT MAX(AccessTransactionID) FROM ACCESSTRANSACTIONS ", conn);
            object result = comm.ExecuteScalar();

            if (conn != null)
            {
                conn.Dispose();
            }

            if (result == null || result == DBNull.Value)
            {
                lastUpdatedTransaction = 0;
            }
            else
            {
                lastUpdatedTransaction = Convert.ToInt32(result);
            }

            return lastUpdatedTransaction;
        }

        private static void MapEmployeeWithTimesheet(string lmsConnection)
        {
            using (SqlConnection conn = new SqlConnection(lmsConnection))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_MapEmployee_Timesheet", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static DataTable GetMsAccessDetails(string msAccessConnection, int lastUpdatedTransaction)
        {
            WriteLog("Processing MS Access Details");

            DataTable accessResults = new DataTable();
            using (OleDbConnection conn = new OleDbConnection(msAccessConnection))
            {
                string strSqlquery = "";

                WriteLog("Processing transactions from " + lastUpdatedTransaction + " onwards from MS Access Database");
                strSqlquery = "SELECT Trans.Tid, Trans.CARDID, Trans.Dt, CBool(Trans.InOut), NULL, Trans.Tid FROM Trans WHERE Trans.Tid > " + lastUpdatedTransaction;

                OleDbCommand cmd = new OleDbCommand(strSqlquery, conn);

                conn.Open();

                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);

                adapter.Fill(accessResults);
            }

            return accessResults;
        }

        private static void BulkInsertToTimeSheet(string lmsConnection, DataTable accessResults)
        {
            using (SqlConnection connection =
                new SqlConnection(lmsConnection))
            {
                // make sure to enable triggers
                SqlBulkCopy bulkCopy =
                    new SqlBulkCopy
                        (
                            connection,
                            SqlBulkCopyOptions.TableLock |
                            SqlBulkCopyOptions.FireTriggers |
                            SqlBulkCopyOptions.UseInternalTransaction,
                            null
                        )
                    { DestinationTableName = "ACCESSTRANSACTIONS" };

                // set the destination table name
                connection.Open();

                // write the data in the "dataTable"
                bulkCopy.WriteToServer(accessResults);
            }
        }

        private static void WriteLog(string message)
        {
            string formattedmsg = DateTime.Now + ": " + message;
            Console.WriteLine(formattedmsg);
            WriteLogFile(formattedmsg);
        }

        private static void WriteLogFile(string formattedMsg)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\LMSTimeSheet.log";
                if (Directory.Exists(Path.GetDirectoryName(path)))
                {
                    File.AppendAllText(path, formattedMsg + Environment.NewLine);
                }
                else
                    Console.WriteLine("Failed to write to log file file for the path  " + path + ".");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}