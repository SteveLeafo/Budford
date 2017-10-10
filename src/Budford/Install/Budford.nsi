;--------------------------------
!include "MUI.nsh"
!include "LogicLib.nsh"
!include "x64.nsh"
!include "FileAssociation.nsh"
;--------------------------------

; The following variables are initialised in OnInit
Var BudfordSoftwareTitle
Var BudfordSoftwareInstallerExe
Var AppDataPath
	

Name "Budford"
OutFile "Budford - Setup.exe"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\Budford"

;Default installation folder
InstallDir "$PROGRAMFILES64\Budford"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" "Path"

;Set Compression Type
SetCompressor LZMA
;SetCompressor BZIP2
;SetCompressor ZLIB

BrandingText /TRIMCENTER "BudSoft Pty Ltd"

XPStyle on

VIAddVersionKey "ProductName" "Budford - Cemu Configuration Manager"
VIAddVersionKey "CompanyName" "BudSoft Software Pty Ltd"
VIAddVersionKey "LegalCopyright" "BudSoft Software Pty Ltd"
VIAddVersionKey "LegalTrademarks" ""
VIAddVersionKey "ProductVersion" "0.0.1.523"
VIAddVersionKey "FileVersion" "0.0.1.523"
VIAddVersionKey "FileDescription" "Installer for Budford - Cemu Configuration Manager"
VIAddVersionKey "Comments" ""
VIProductVersion "1.0.0.0"




;--------------------------------
;Interface Settings

!define MUI_ABORTWARNING
;!define MUI_HEADERIMAGE_BITMAP_NOSTRETCH
;!define MUI_HEADERIMAGE_BITMAP "..\Common\Images\deswik.logo.bmp"
;!define MUI_HEADERIMAGE
;!define MUI_WELCOMEFINISHPAGE_BITMAP "..\Common\Images\DeswikWizardSideBar.bmp"


!define MUI_WELCOMEPAGE_TITLE "Welcome to Budford - Cemu Configuration Manager"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_TITLE "Completing Budford Installer"
!insertmacro MUI_PAGE_FINISH

;--------------------------------
;Macro creates functions to initialise variables in installer and uninstaller
!macro CommonInitVars UN
Function ${UN}InitVars
	StrCpy $BudfordSoftwareTitle "Budford - Cemu Configuration Manager"
	StrCpy $BudfordSoftwareInstallerExe "$BudfordSoftwareTitle.exe"
FunctionEnd
!macroend

!insertmacro CommonInitVars ""
!insertmacro CommonInitVars "un."

;--------------------------------
;Languages
!insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Reserve Files
!insertmacro MUI_RESERVEFILE_INSTALLOPTIONS

;--------------------------------
;Installer Sections
InstType "Documentation Installation"
InstType /NOCUSTOM

;--------------------------------
;Installer Sections

Section "Full Installation" FullInstallation
SectionIn 1
        SetShellVarContext all
        ;Call DetectRunningApplications
        Call CheckUserIsAdministrator
        Call InstallBudfordFiles
	Call CheckAndInstallDotNet
	Call InstallUninstaller
	Call InstallPreRequisits
	Call InstallFileAssociations
	Call InstallApplicationShortcuts
	
SectionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"
	;Call Un.DetectRunningApplications
  	Call Un.CheckUserIsAdministrator
	Call Un.InstallBudfordFiles
	Call Un.InstallFileAssociations
	Call Un.InstallUninstaller
	Call Un.InstallApplicationShortcuts

SectionEnd



;******************************************************************************
; Creates shortcuts for the application
; Pre: $DeswikSoftwareSuite must be the name of the software suite
;      e.g. "Deswik Software Suite 1.3"
;******************************************************************************
Function InstallApplicationShortcuts
SetShellVarContext all
		
    RMDir /r "$SMPROGRAMS\Budford - Cemu Configuration Manager"
  	

    CreateDirectory "$SMPROGRAMS\Budford - Cemu Configuration Manager"
    SetOutPath "$SMPROGRAMS\Budford - Cemu Configuration Manager"
    CreateShortCut "$SMPROGRAMS\Budford - Cemu Configuration Manager\Budford - Cemu Configuration Manager.lnk" "$INSTDIR\Budford.exe" "" "$INSTDIR\Budford.ico" 0

    ;IfFileExists "$INSTDIR\Budford.exe" 0 +2
		   	      
FunctionEnd 

;******************************************************************************
; Deletes shortcuts for the application
;******************************************************************************
Function Un.InstallApplicationShortcuts
    SetShellVarContext all    
    RMDir /r "$SMPROGRAMS\Budford - Cemu Configuration Manager"   
FunctionEnd


;******************************************************************************
; Sets up the registry entries needed to support Add/Remove programs
;******************************************************************************
Function InstallUninstaller 
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Budford - Cemu Configuration Manager"	

    WriteUninstaller "$INSTDIR\Uninstall $BudfordSoftwareInstallerExe"
    WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "Path" "$INSTDIR"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$BudfordSoftwareTitle" "DisplayName" "$BudfordSoftwareTitle"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$BudfordSoftwareTitle" "UninstallString" "$INSTDIR\Uninstall $BudfordSoftwareInstallerExe"
    
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$BudfordSoftwareTitle" "Publisher" "BudSoft"
  	
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$BudfordSoftwareTitle" "DisplayVersion" "1.0.0.0"
  			
			
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$BudfordSoftwareTitle" "NoModify" "1"
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$BudfordSoftwareTitle" "NoRepair" "1"
FunctionEnd

;******************************************************************************
; Removes the registry entries needed to support Add/Remove programs
;******************************************************************************
Function Un.InstallUninstaller
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$BudfordSoftwareTitle"
	DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
FunctionEnd


;******************************************************************************
; Initialisation callback installer (doesn't get run during uninstall - see un.onInit)
;******************************************************************************
Function .onInit
	Call InitVars
FunctionEnd

;******************************************************************************
; Initialisation callback for uninstaller
;******************************************************************************
Function un.onInit
	Call un.InitVars
FunctionEnd

;*********************************************************************
; Installs documentation files for the software suite
;*********************************************************************
Function InstallBudfordFiles

    CreateDirectory "$INSTDIR\Users"
	SetOutPath "$INSTDIR\Users"
	File "Users\*.*"
	
	!define CSIDL_COMMON_APPDATA 0x0023        
	System::Call "shell32::SHGetFolderPath(0, i ${CSIDL_COMMON_APPDATA}, 0, 0, t .r1)"  
	StrCpy $AppDataPath "$1"

	AccessControl::GrantOnFile "$AppDataPath\Budford" "(BU)" "GenericRead + GenericWrite"
	SetOutPath "$AppDataPath\Budford"
	File "default.bin"
	File "OldVersions.xml"
	File "SaveDirDatabase.xml"
	File "sharedFonts.zip"
	;File "shaderCache.zip"
	File "graphicsPacks.zip"
	File "controllerProfiles.zip"
	File "sys.zip"
	File "wiiutdb.xml"
	AccessControl::GrantOnFile "$AppDataPath\Budford\SaveDirDatabase.xml" "(BU)" "GenericRead + GenericWrite"
	AccessControl::GrantOnFile "$AppDataPath\Budford\OldVersions.xml" "(BU)" "GenericRead + GenericWrite"
	

	
	CreateDirectory "$AppDataPath\Budford\Users"
	AccessControl::GrantOnFile "$AppDataPath\Budford\Users" "(BU)" "GenericRead + GenericWrite"
	SetOutPath "$AppDataPath\Budford\Users"
	File "Users\*.*"

	
    CreateDirectory "$INSTDIR"
	SetOutPath "$INSTDIR"
	File "Budford.exe"
	File "Budford.ico"
FunctionEnd

;*********************************************************************
; Uninstalls documentation files for the software suite
;*********************************************************************
Function Un.InstallBudfordFiles
	SetShellVarContext all
	Delete "$INSTDIR\*.*"
	Delete "$INSTDIR\Users\*.*"
	
	RMDir "$INSTDIR\Users"
	RMDir "$INSTDIR"
FunctionEnd


;******************************************************************************
; Registers the file associations for files used by the installed applications
;******************************************************************************
Function InstallFileAssociations    
	; Register the Deswik CAD files
	${registerExtension} "$INSTDIR\Budford.exe" ".rpx" "Wii-U Game Files"		
FunctionEnd

;******************************************************************************
; Unregisters the file associations for files used by the installed applications
;******************************************************************************
Function Un.InstallFileAssociations    
    ; Unregister the Deswik CAD files
    ${unregisterExtension} ".rpx" "Wii-U Game Files"	     
FunctionEnd


;******************************************************************************
; Checks for and installs .NET
;******************************************************************************
Function CheckAndInstallDotNet
    ; Magic numbers from http://msdn.microsoft.com/en-us/library/ee942965.aspx
    ClearErrors
    ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"

    IfErrors NotDetected

    ${If} $0 >= 378389
        DetailPrint "Microsoft .NET Framework 4.5 is installed ($0)"
    ${Else}
    NotDetected:
        DetailPrint "Installing Microsoft .NET Framework 4.5"
        SetDetailsPrint listonly
        ExecWait '"$INSTDIR\Tools\dotNetFx45_Full_setup.exe" /passive /norestart' $0
        ${If} $0 == 3010 
        ${OrIf} $0 == 1641
            DetailPrint "Microsoft .NET Framework 4.5 installer requested reboot"
            SetRebootFlag true
        ${EndIf}
        SetDetailsPrint lastused
        DetailPrint "Microsoft .NET Framework 4.5 installer returned $0"
    ${EndIf}

FunctionEnd


;******************************************************************************
; Aborts the installation if the user is not an administrator
; This code has been adapted from an example on the NSIS website.
;******************************************************************************
Function CheckUserIsAdministrator
    # call userInfo plugin to get user info.  The plugin puts the result in the stack
        userInfo::getAccountType

        # pop the result from the stack into $0
        pop $0

        # compare the result with the string "Admin" to see if the user is admin.
        # If match, jump 3 lines down.
        strCmp $0 "Admin" +3

        # if there is not a match, print message and return
    messageBox MB_OK "Please log in as an administrator of this computer to install this software."
    Abort
FunctionEnd

;******************************************************************************
; See Un.CheckUserIsAdministrator
;******************************************************************************
Function Un.CheckUserIsAdministrator
    # call userInfo plugin to get user info.  The plugin puts the result in the stack
        userInfo::getAccountType

        # pop the result from the stack into $0
        pop $0

        # compare the result with the string "Admin" to see if the user is admin.
        # If match, jump 3 lines down.
        strCmp $0 "Admin" +3

        # if there is not a match, print message and return
    messageBox MB_OK "Please log in as an administrator of this computer to uninstall this software."
    Abort
FunctionEnd

;******************************************************************************
; See InstallPreRequisits
;******************************************************************************
Function InstallPreRequisits
ClearErrors

ReadRegStr $0 HKLM "SOFTWARE\Classes\Installer\Dependencies\{d992c12e-cab2-426f-bde3-fb8c53950b0d}" ""

${If} ${Errors}

	# key does not exist
	download1:
	NSISdl::download "https://download.microsoft.com/download/6/A/A/6AA4EDFF-645B-48C5-81CC-ED5963AEAD48/vc_redist.x64.exe"  "$Temp/vcredist_x64.exe"
	pop $0
	StrCmp "$0" "success" execStep1 instAbort1
	execStep1:
	Execwait '"$Temp/vcredist_x64.exe" /q' ; '/q' to install silently
	pop $0
	StrCmp "$0" "success" installed execStep1
	instAbort1:
	StrCmp $0 "cancel" 0 +1
	MessageBox MB_OKCANCEL "Connection Timed Out. Retry ? " IDOK  download1 IDCANCEL 0
	Quit
	;not installed, so run the installer
	;ExecWait 'MyPathWhereInstallerIs\vc++2010setup.exe'
${EndIf}

	installed:

;we are done
FunctionEnd