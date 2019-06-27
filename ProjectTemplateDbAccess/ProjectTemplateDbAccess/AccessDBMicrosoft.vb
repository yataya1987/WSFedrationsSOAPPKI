Imports System
Imports System.IO
Imports System.Collections

Module AccessMS



	Public Class AccessDBMicrosoft


		Public Shared Sub Main()

			RunMacro___()

		End Sub

		Public Function RunMacro___()

			Dim fso As FileSystemObject
			Dim sSourcePath As String
			Dim sSourceFile As String
			Dim sBackupPath As String
			Dim sBackupFile As String
			Dim DB As New Access.Application

			sSourcePath = "C:\source\"
			sSourceFile = "DBAdv.mdb"
			sBackupPath = "C:\backup\"
			sBackupFile = "BackupDB_" & Format(Now, "mmddyyyy") & "_" & Format(Time, "hhmmss") & ".mdb"

			fso = New FileSystemObject

			fso.CopyFile(sSourcePath & sSourceFile, sBackupPath & sBackupFile, True)

			fso = Nothing

			MsgBox("Backup was successful and saved @ " & Chr(13) & Chr(13) & sBackupPath & Chr(13) & Chr(13) & "The backup file name is " & Chr(13) & Chr(13) & sBackupFile, vbInformation, "Backup Completed")


		End Function


	End Class
End Module