<#
.SYNOPSIS
    发布 MyDiary.UI 应用程序到不同平台。

.DESCRIPTION
    此脚本用于发布 MyDiary.UI 应用程序到 Windows、Linux、Browser 和 Android 平台。
    
.PARAMETER c
    设置此标志以创建一个自包含的发布版本。

.PARAMETER s
    设置此标志以在发布版本中包含所有文件。

.PARAMETER w
    发布到 Windows 平台。

.PARAMETER l
    发布到 Linux 平台。

.PARAMETER b
    发布到 Browser 平台。

.PARAMETER a
    发布到 Android 平台。

.PARAMETER akeyfile
    Android 平台签名文件的路径。

.PARAMETER akeyname
    Android 平台签名别名。

.PARAMETER akeypswd
    Android 平台签名密码。

.EXAMPLE
    .\YourScript.ps1 -w -c -s
    发布到 Windows 平台，创建自包含版本，并包含所有文件。

.EXAMPLE
    .\YourScript.ps1 -l
    发布到 Linux 平台。

.EXAMPLE
    .\YourScript.ps1 -b
    发布到 Browser 平台。

.EXAMPLE
    .\YourScript.ps1 -a -akeyfile "path\to\your\keystore.keystore" -akeyname "yourAlias" -akeypswd "yourPassword"
    发布到 Android 平台，并指定签名文件、别名和密码。

#>

# 上面都是ChatGPT写的，牛逼！

param(
    [switch]$w,
    [switch]$l,
    [switch]$b,
    [switch]$a,
    [switch]$c,
    [switch]$s ,
    [string]$akeyfile,
    [string]$akeyname,
    [string]$akeypswd
)

try {
    # 检查是否提供了至少一个平台的发布开关
    if (-not ($w -or $l -or $b -or $a)) {
        Write-Host "请提供至少一个平台的发布开关 (-w, -l, -b, -a)"
        Get-Help $MyInvocation.MyCommand.Path   
        return
    }
    
    # 检查.NET SDK是否安装
    if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
        throw "未安装.NET SDK"
    }

    # 检查目录结构是否存在，如果不存在则创建
    $publishDirectory = "Publish"
    if (-not (Test-Path $publishDirectory -PathType Container)) {
        New-Item -Path $publishDirectory -ItemType Directory | Out-Null
    }
    
    Clear-Host
    if ($w) {
        Write-Output "正在发布win-x64"
        dotnet publish MyDiary.UI/MyDiary.UI.Desktop -r win-x64 -c Release -o Publish/win-x64 --self-contained $c /p:PublishSingleFile=$s 
    }
    if ($l) {
        Write-Output "正在发布linux-x64"
        dotnet publish MyDiary.UI/MyDiary.UI.Desktop -r linux-x64 -c Release -o Publish/linux-x64 --self-contained $c /p:PublishSingleFile=$s 
    }
    if ($b) {
        Write-Output "正在发布browser-wasm"
        dotnet publish MyDiary.UI/MyDiary.UI.Browser -r browser-wasm -c Release -o Publish/web --self-contained true
        Copy-Item -Recurse $Env:TEMP/MyDiary_Release\MyDiary.UI.Browser\net8.0\browser-wasm/AppBundle Publish/web
    }
    if ($a) {
        dotnet publish MyDiary.UI/MyDiary.UI.Android -f net8.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore= $akeyfile.keystore -p:AndroidSigningKeyAlias=$akeyname -p:AndroidSigningKeyPass=$akeypswd  -p:AndroidSigningStorePass=$akeypswd 
        $publishAndroidDirectory = "Publish/android"
        if (-not (Test-Path $publishAndroidDirectory -PathType Container)) {
            New-Item -Path $publishAndroidDirectory -ItemType Directory | Out-Null
        }
        Copy-Item -Recurse $Env:TEMP/MyDiary_Release\MyDiary.UI.Android\net8.0-android\publish\* Publish/android
    }
    Write-Output "操作完成"
    Write-Output "Windows：双击MyDiary.UI.Desktop.exe运行"
    Write-Output "Linux：执行./MyDiary.UI.Desktop"
    Write-Output "Web：首先确认已安装dotnet serve（安装：dotnet tool install --global dotnet-serve），然后执行dotnet serve -d:./AppBundle"
    Write-Output "Android：安装.apk"
    Invoke-Item Publish
    pause
}
catch {
    Write-Error $_
}