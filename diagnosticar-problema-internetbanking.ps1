Write-Host "🔍 DIAGNÓSTICO: PROBLEMA INTERNET BANKING vs COLLECTION POSTMAN" -ForegroundColor Yellow
Write-Host "=================================================================" -ForegroundColor Yellow
Write-Host ""

# Função para fazer requisições HTTP
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [hashtable]$Headers = @{},
        [string]$Body = $null
    )
    
    try {
        $params = @{
            Method = $Method
            Uri = $Uri
            Headers = $Headers
            ContentType = "application/json"
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        $response = Invoke-RestMethod @params
        return @{ Success = $true; Data = $response; Error = $null }
    }
    catch {
        return @{ Success = $false; Data = $null; Error = $_.Exception.Message }
    }
}

Write-Host "ETAPA 1: Testando login admin..." -ForegroundColor Cyan
$adminLogin = @{
    email = "admin@fintechpsp.com"
    password = "admin123"
} | ConvertTo-Json

$adminResult = Invoke-ApiRequest -Method "POST" -Uri "http://localhost:5000/auth/login" -Body $adminLogin

if ($adminResult.Success) {
    Write-Host "✅ Admin login OK" -ForegroundColor Green
    $adminToken = $adminResult.Data.accessToken
    $adminHeaders = @{ "Authorization" = "Bearer $adminToken" }
    
    Write-Host ""
    Write-Host "ETAPA 2: Verificando usuários existentes..." -ForegroundColor Cyan
    $usersResult = Invoke-ApiRequest -Method "GET" -Uri "http://localhost:5000/client-users" -Headers $adminHeaders
    
    if ($usersResult.Success) {
        Write-Host "✅ Endpoint /client-users funcionando" -ForegroundColor Green
        $users = $usersResult.Data.users
        Write-Host "📊 Total de usuários: $($users.Count)" -ForegroundColor White
        
        $clienteExiste = $users | Where-Object { $_.email -eq "cliente@empresateste.com" }
        
        if ($clienteExiste) {
            Write-Host "⚠️ Usuário cliente@empresateste.com JÁ EXISTE!" -ForegroundColor Yellow
            Write-Host "📋 Dados existentes:" -ForegroundColor Cyan
            Write-Host "   ID: $($clienteExiste.id)" -ForegroundColor White
            Write-Host "   Nome: $($clienteExiste.name)" -ForegroundColor White
            Write-Host "   Email: $($clienteExiste.email)" -ForegroundColor White
            Write-Host "   Ativo: $($clienteExiste.active)" -ForegroundColor White
            Write-Host "   Role: $($clienteExiste.role)" -ForegroundColor White
            
            Write-Host ""
            Write-Host "ETAPA 3: Testando login do cliente existente..." -ForegroundColor Cyan
            $clientLogin = @{
                email = "cliente@empresateste.com"
                password = "123456"
            } | ConvertTo-Json
            
            $clientResult = Invoke-ApiRequest -Method "POST" -Uri "http://localhost:5000/auth/login" -Body $clientLogin
            
            if ($clientResult.Success) {
                Write-Host "✅ Cliente login OK" -ForegroundColor Green
                $clientToken = $clientResult.Data.accessToken
                $clientHeaders = @{ "Authorization" = "Bearer $clientToken" }
                
                Write-Host "👤 Usuário logado: $($clientResult.Data.user.name)" -ForegroundColor Green
                Write-Host "🔑 Role: $($clientResult.Data.user.role)" -ForegroundColor Green
                Write-Host "🆔 ID: $($clientResult.Data.user.id)" -ForegroundColor Green
                
                Write-Host ""
                Write-Host "ETAPA 4: Testando /client-users/me (usado pelo InternetBanking)..." -ForegroundColor Cyan
                $meResult = Invoke-ApiRequest -Method "GET" -Uri "http://localhost:5000/client-users/me" -Headers $clientHeaders
                
                if ($meResult.Success) {
                    Write-Host "✅ Endpoint /client-users/me funcionando!" -ForegroundColor Green
                    Write-Host "📋 Dados retornados:" -ForegroundColor Cyan
                    Write-Host "   ID: $($meResult.Data.id)" -ForegroundColor White
                    Write-Host "   Nome: $($meResult.Data.name)" -ForegroundColor White
                    Write-Host "   Email: $($meResult.Data.email)" -ForegroundColor White
                    Write-Host "   Role: $($meResult.Data.role)" -ForegroundColor White
                    Write-Host "   Ativo: $($meResult.Data.active)" -ForegroundColor White
                    
                    Write-Host ""
                    Write-Host "🎯 DIAGNÓSTICO: SISTEMA FUNCIONANDO CORRETAMENTE!" -ForegroundColor Green
                    Write-Host "✅ O problema NÃO está na API" -ForegroundColor Green
                    Write-Host "✅ O problema pode estar no InternetBankingWeb frontend" -ForegroundColor Yellow
                    Write-Host ""
                    Write-Host "🔧 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
                    Write-Host "1. Verificar se o InternetBankingWeb está conectando na URL correta" -ForegroundColor White
                    Write-Host "2. Verificar logs do navegador (F12 > Console)" -ForegroundColor White
                    Write-Host "3. Verificar se há problemas de CORS" -ForegroundColor White
                    Write-Host "4. Verificar se o token está sendo salvo corretamente no localStorage" -ForegroundColor White
                    
                } else {
                    Write-Host "❌ Erro no endpoint /client-users/me: $($meResult.Error)" -ForegroundColor Red
                    Write-Host "🔍 ESTE É O PROBLEMA! InternetBanking não consegue obter dados do usuário" -ForegroundColor Yellow
                }
                
            } else {
                Write-Host "❌ Erro no login do cliente: $($clientResult.Error)" -ForegroundColor Red
                Write-Host "🔍 Problema: Senha incorreta ou usuário inativo" -ForegroundColor Yellow
                
                Write-Host ""
                Write-Host "ETAPA 3.1: Tentando criar usuário com senha via /admin/users..." -ForegroundColor Cyan
                $createUserAdmin = @{
                    name = "Cliente EmpresaTeste"
                    email = "cliente@empresateste.com"
                    password = "123456"
                    role = "cliente"
                    isActive = $true
                } | ConvertTo-Json
                
                $createResult = Invoke-ApiRequest -Method "POST" -Uri "http://localhost:5000/admin/users" -Headers $adminHeaders -Body $createUserAdmin
                
                if ($createResult.Success) {
                    Write-Host "✅ Usuário criado via /admin/users" -ForegroundColor Green
                    Write-Host "🔄 Tentando login novamente..." -ForegroundColor Cyan
                    
                    $clientResult2 = Invoke-ApiRequest -Method "POST" -Uri "http://localhost:5000/auth/login" -Body $clientLogin
                    
                    if ($clientResult2.Success) {
                        Write-Host "✅ Login funcionou após criar via /admin/users!" -ForegroundColor Green
                        Write-Host "🎯 SOLUÇÃO: Usar /admin/users para criar usuários com senha" -ForegroundColor Yellow
                    } else {
                        Write-Host "❌ Login ainda falhou: $($clientResult2.Error)" -ForegroundColor Red
                    }
                } else {
                    Write-Host "❌ Erro ao criar via /admin/users: $($createResult.Error)" -ForegroundColor Red
                }
            }
            
        } else {
            Write-Host "ℹ️ Usuário cliente@empresateste.com NÃO EXISTE" -ForegroundColor Blue
            Write-Host ""
            Write-Host "ETAPA 3: Criando usuário via /client-users..." -ForegroundColor Cyan
            $createUser = @{
                name = "Cliente EmpresaTeste"
                email = "cliente@empresateste.com"
                document = "12345678901"
                phone = "(11) 99999-9999"
                address = "Rua Teste, 123 - São Paulo/SP"
            } | ConvertTo-Json
            
            $createResult = Invoke-ApiRequest -Method "POST" -Uri "http://localhost:5000/client-users" -Headers $adminHeaders -Body $createUser
            
            if ($createResult.Success) {
                Write-Host "✅ Usuário criado via /client-users" -ForegroundColor Green
                Write-Host "⚠️ Mas sem senha! Precisa criar via /admin/users também" -ForegroundColor Yellow
                
                Write-Host ""
                Write-Host "ETAPA 3.1: Criando usuário com senha via /admin/users..." -ForegroundColor Cyan
                $createUserAdmin = @{
                    name = "Cliente EmpresaTeste"
                    email = "cliente@empresateste.com"
                    password = "123456"
                    role = "cliente"
                    isActive = $true
                } | ConvertTo-Json
                
                $createAdminResult = Invoke-ApiRequest -Method "POST" -Uri "http://localhost:5000/admin/users" -Headers $adminHeaders -Body $createUserAdmin
                
                if ($createAdminResult.Success) {
                    Write-Host "✅ Usuário com senha criado via /admin/users" -ForegroundColor Green
                } else {
                    Write-Host "❌ Erro ao criar via /admin/users: $($createAdminResult.Error)" -ForegroundColor Red
                }
            } else {
                Write-Host "❌ Erro ao criar via /client-users: $($createResult.Error)" -ForegroundColor Red
            }
        }
        
    } else {
        Write-Host "❌ Erro ao listar usuários: $($usersResult.Error)" -ForegroundColor Red
    }
    
} else {
    Write-Host "❌ Erro no login admin: $($adminResult.Error)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🏁 DIAGNÓSTICO CONCLUÍDO" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
