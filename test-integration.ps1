# Script para testar a integração PIX com Sicoob
Write-Host "🎯 Testando integração PIX com Sicoob..."

# Configurar headers
$headers = @{
    'Authorization' = 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImp0aSI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsImlhdCI6MTcyNzg5NzQwMCwibmJmIjoxNzI3ODk3NDAwLCJleHAiOjE3Mjc5ODM4MDAsImlzcyI6Ik1vcnRhZGVsYSIsImF1ZCI6Ik1vcnRhZGVsYSJ9.Hs_pN8Qs8vF7Hs_pN8Qs8vF7Hs_pN8Qs8vF7Hs_pN8Q'
    'Content-Type' = 'application/json'
}

# Dados da transação PIX
$pixData = @{
    externalId = "PIX-INTEGRATION-TEST-$(Get-Date -Format 'yyyyMMddHHmmss')"
    amount = 25.50
    pixKey = "11999887766"
    bankCode = "756"
    description = "Teste integração PIX-Sicoob via MassTransit"
    webhookUrl = "https://webhook.site/test-integration"
} | ConvertTo-Json

Write-Host "📋 Dados da transação:"
Write-Host $pixData

# Tentar criar transação
try {
    Write-Host "🚀 Enviando requisição para TransactionService..."
    $response = Invoke-RestMethod -Uri "http://localhost:5002/api/transactions/pix" -Method POST -Headers $headers -Body $pixData -ErrorAction Stop
    
    Write-Host "✅ Transação PIX criada com sucesso!"
    Write-Host "   ID: $($response.id)"
    Write-Host "   ExternalId: $($response.externalId)"
    Write-Host "   Status: $($response.status)"
    Write-Host "   Valor: R$ $($response.amount)"
    
    # Aguardar processamento
    Write-Host "⏳ Aguardando processamento da integração..."
    Start-Sleep -Seconds 5
    
    # Verificar logs do IntegrationService
    Write-Host "📋 Verificando logs do IntegrationService..."
    & docker logs fintech-integration-service --tail 10

} catch {
    Write-Host "❌ Erro ao criar transação PIX:"
    Write-Host "   Mensagem: $($_.Exception.Message)"
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "   Resposta do servidor: $responseBody"
    }
}

Write-Host "🏁 Teste concluído."
