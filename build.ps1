param(
[switch]$c=$false ,
[switch]$s=$true
)

try {
    try {
        dotnet
    }
    catch {
        throw "未安装.NET SDK"
    }
    
    Clear-Host
   
    Write-Output "正在发布win-x64"
    dotnet publish MyDiary.UI/MyDiary.UI.Desktop -r win-x64 -c Release -o Publish/win-x64 --self-contained $c /p:PublishSingleFile=$s 
    
    Write-Output "正在发布linux-x64"
    dotnet publish MyDiary.UI/MyDiary.UI.Desktop -r linux-x64 -c Release -o Publish/linux-x64 --self-contained $c /p:PublishSingleFile=$s 
    
    Write-Output "正在发布browser-wasm"
    dotnet publish MyDiary.UI/MyDiary.UI.Browser -r browser-wasm -c Release -o Publish/web --self-contained true
    Copy-Item -Recurse $Env:TEMP/MyDiary_Release\MyDiary.UI.Browser\net8.0\browser-wasm/AppBundle Publish/web
    Write-Output "操作完成"
    Write-Output "Windows：双击MyDiary.UI.Desktop.exe运行"
    Write-Output "Linux：执行./MyDiary.UI.Desktop"
    Write-Output "Web：首先确认已安装dotnet serve（安装：dotnet tool install --global dotnet-serve），然后执行dotnet serve -d:./AppBundle"
    Invoke-Item Publish
    pause
}
catch {
    Write-Error $_
}