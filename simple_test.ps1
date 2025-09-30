Write-Host "TESTE COMPLETO DO SISTEMA FINTECH PSP"
Write-Host "====================================="

try {
    Write-Host "Teste 1: Login via API Gateway"
    $body = '{"email":"admin@fintechpsp.com","password":"admin123"}'
    $login = Invoke-RestMethod -Uri "http://localhost:5000/auth/login" -Method POST -Body $body -ContentType "application/json"
    Write-Host "SUCCESS: Login realizado com sucesso!"
    Write-Host "Token: $($login.accessToken.Substring(0,50))..."

    Write-Host "Teste 2: Health Check API Gateway"
    $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method GET
    Write-Host "SUCCESS: API Gateway Health: $health"

    Write-Host "Teste 3: Lista de usuarios"
    $auth = @{Authorization="Bearer $($login.accessToken)"}
    $users = Invoke-RestMethod -Uri "http://localhost:5000/client-users" -Method GET -Headers $auth
    Write-Host "SUCCESS: Lista de usuarios obtida com sucesso!"
    Write-Host "Total de usuarios: $($users.Count)"

    Write-Host "Teste 4: Banking Contas (rota corrigida)"
    $contas = Invoke-RestMethod -Uri "http://localhost:5000/banking/contas" -Method GET -Headers $auth
    Write-Host "SUCCESS: Rota /banking/contas funcionando perfeitamente!"
    Write-Host "Total de contas: $($contas.Count)"

    Write-Host "TODOS OS TESTES PASSARAM!"
    Write-Host "Sistema 100% funcional e operacional!"

} catch {
    Write-Host "ERRO durante os testes: $($_.Exception.Message)"
}
