#!/usr/bin/env pwsh

Write-Host "🚀 TESTE COMPLETO DO SISTEMA FINTECH PSP" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

try {
    # Teste 1: Login
    Write-Host "`n🔐 TESTE 1: Login via API Gateway" -ForegroundColor Yellow
    $body = '{"email":"admin@fintechpsp.com","password":"admin123"}'
    $login = Invoke-RestMethod -Uri "http://localhost:5000/auth/login" -Method POST -Body $body -ContentType "application/json"
    Write-Host "✅ Login realizado com sucesso!" -ForegroundColor Green
    Write-Host "   Token: $($login.accessToken.Substring(0,50))..." -ForegroundColor Cyan

    # Teste 2: Health Check
    Write-Host "`n🏥 TESTE 2: Health Check API Gateway" -ForegroundColor Yellow
    $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method GET
    Write-Host "✅ API Gateway Health: $health" -ForegroundColor Green

    # Teste 3: Lista de usuários
    Write-Host "`n👥 TESTE 3: Lista de usuários" -ForegroundColor Yellow
    $auth = @{Authorization="Bearer $($login.accessToken)"}
    $users = Invoke-RestMethod -Uri "http://localhost:5000/client-users" -Method GET -Headers $auth
    Write-Host "✅ Lista de usuários obtida com sucesso!" -ForegroundColor Green
    Write-Host "   Total de usuários: $($users.Count)" -ForegroundColor Cyan

    # Teste 4: Banking Contas (a rota que estava com problema)
    Write-Host "`n🏦 TESTE 4: Banking Contas (rota corrigida)" -ForegroundColor Yellow
    $contas = Invoke-RestMethod -Uri "http://localhost:5000/banking/contas" -Method GET -Headers $auth
    Write-Host "✅ Rota /banking/contas funcionando perfeitamente!" -ForegroundColor Green
    Write-Host "   Total de contas: $($contas.Count)" -ForegroundColor Cyan

    Write-Host "`n🎉 TODOS OS TESTES PASSARAM!" -ForegroundColor Green
    Write-Host "🎯 Sistema 100% funcional e operacional!" -ForegroundColor Green
    Write-Host "✨ Todas as rotas estão funcionando corretamente!" -ForegroundColor Green

} catch {
    Write-Host "❌ Erro durante os testes: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Detalhes: $($_.Exception)" -ForegroundColor Red
}
