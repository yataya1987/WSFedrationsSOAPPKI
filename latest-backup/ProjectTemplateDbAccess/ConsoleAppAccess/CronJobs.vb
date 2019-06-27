Imports System
Imports System.IO
Imports System.Collections
Imports ConsoleAppAccess
Imports Microsoft.VisualBasic
Imports Microsoft.Office.Interop.Access
Imports Microsoft.Office.Interop

Module CronJobs

	Sub Main()
			Dim fso As Scripting.FileSystemObject
			Dim sSourcePath As String
			Dim sSourceFile As String
			Dim sBackupPath As String
			Dim sBackupFile As String


		sSourcePath = "D:\paltel_collection\App_Data"
		sSourceFile = "DBAdv.mdb"
			sBackupPath = "C:\backup\"
		sBackupFile = "BackupDB_" & Format(Now, "mmddyyyy") & "_" & Format(DateTime.Now, "hhmmss") & ".mdb"

		fso = New Scripting.FileSystemObject

			fso.CopyFile(sSourcePath & sSourceFile, sBackupPath & sBackupFile, True)

			fso = Nothing

			MsgBox("Backup was successful and saved @ " & Chr(13) & Chr(13) & sBackupPath & Chr(13) & Chr(13) & "The backup file name is " & Chr(13) & Chr(13) & sBackupFile, vbInformation, "Backup Completed")

		End Sub


End Module
