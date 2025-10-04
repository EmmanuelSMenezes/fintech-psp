Write-Host "🎯 Testando integração PIX com Sicoob..."

$headers = @{
    'Authorization' = 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImp0aSI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsImlhdCI6MTcyNzg5NzQwMCwibmJmIjoxNzI3ODk3NDAwLCJleHAiOjE3Mjc5ODM4MDAsImlzcyI6Ik1vcnRhZGVsYSIsImF1ZCI6Ik1vcnRhZGVsYSJ9.Hs_pN8Qs8vF7Hs_pN8Qs8vF7Hs_pN8Qs8vF7Hs_pN8Q'
    'Content-Type' = 'application/json'
}

$pixData = @{
    externalId = "PIX-TEST-20251002231500"
    amount = 25.50
    pixKey = "11999887766"
    bankCode = "756"
    description = "Teste integração PIX-Sicoob"
    webhookUrl = "https://webhook.site/test"
} | ConvertTo-Json

Write-Host "📋 Criando transação PIX..."
Write-Host $pixData

$response = Invoke-RestMethod -Uri "http://localhost:5002/api/transactions/pix" -Method POST -Headers $headers -Body $pixData
Write-Host "✅ Resposta:"
Write-Host ($response | ConvertTo-Json -Depth 3)

Write-Host "⏳ Aguardando 3 segundos..."
Start-Sleep -Seconds 3

Write-Host "📋 Logs do IntegrationService:"
docker logs fintech-integration-service --tail 5
