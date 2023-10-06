param(
    [String]
    $region1FunctionAppId,
    [String]
    $region2FunctionAppId
)

while ($true) {
    Write-Host "Stopping $region1FunctionAppId"
    az functionapp stop --ids $region1FunctionAppId
    Write-Host "Starting $region2FunctionAppId"
    az functionapp start --ids $region2FunctionAppId
    Start-Sleep -Seconds 180
    
    Write-Host "Stopping $region2FunctionAppId"
    az functionapp stop --ids $region2FunctionAppId
    Write-Host "Starting $region1FunctionAppId"
    az functionapp start --ids $region1FunctionAppId
    Start-Sleep -Seconds 180
}