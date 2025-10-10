# Script para atualizar configuração do Ocelot para usar localhost
Write-Host "🔧 Atualizando configuração do API Gateway..." -ForegroundColor Yellow

$ocelotPath = "src/Gateway/FintechPSP.APIGateway/ocelot.json"
$content = Get-Content $ocelotPath -Raw

# Mapeamento de serviços para portas localhost
$serviceMapping = @{
    '"Host": "user-service", "Port": 8080' = '"Host": "localhost", "Port": 5006'
    '"Host": "config-service", "Port": 8080' = '"Host": "localhost", "Port": 5007'
    '"Host": "balance-service", "Port": 8080' = '"Host": "localhost", "Port": 5003'
    '"Host": "transaction-service", "Port": 8080' = '"Host": "localhost", "Port": 5002'
    '"Host": "webhook-service", "Port": 8080' = '"Host": "localhost", "Port": 5004'
    '"Host": "integration-service", "Port": 8080' = '"Host": "localhost", "Port": 5005'
    '"Host": "company-service", "Port": 8080' = '"Host": "localhost", "Port": 5009'
}

foreach ($old in $serviceMapping.Keys) {
    $new = $serviceMapping[$old]
    $content = $content -replace [regex]::Escape($old), $new
    Write-Host "Substituido: $old -> $new" -ForegroundColor Green
}

# Salvar arquivo atualizado
$content | Set-Content $ocelotPath -Encoding UTF8
Write-Host "Configuracao do Ocelot atualizada!" -ForegroundColor Green
