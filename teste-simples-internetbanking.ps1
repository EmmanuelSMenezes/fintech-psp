Write-Host "DIAGNOSTICO INTERNET BANKING" -ForegroundColor Yellow
Write-Host "============================" -ForegroundColor Yellow

# Login admin
$adminLogin = '{"email":"admin@fintechpsp.com","password":"admin123"}'
$adminResponse = Invoke-RestMethod -Uri "http://localhost:5000/auth/login" -Method POST -Body $adminLogin -ContentType "application/json"
Write-Host "Admin login OK" -ForegroundColor Green

$adminHeaders = @{"Authorization" = "Bearer $($adminResponse.accessToken)"}

# Listar usuarios
$users = Invoke-RestMethod -Uri "http://localhost:5000/client-users" -Method GET -Headers $adminHeaders
Write-Host "Usuarios encontrados: $($users.users.Count)" -ForegroundColor Cyan

$cliente = $users.users | Where-Object { $_.email -eq "cliente@empresateste.com" }

if ($cliente) {
    Write-Host "Cliente existe: $($cliente.name)" -ForegroundColor Green
    
    # Testar login cliente
    $clientLogin = '{"email":"cliente@empresateste.com","password":"123456"}'
    try {
        $clientResponse = Invoke-RestMethod -Uri "http://localhost:5000/auth/login" -Method POST -Body $clientLogin -ContentType "application/json"
        Write-Host "Cliente login OK: $($clientResponse.user.name)" -ForegroundColor Green
        
        $clientHeaders = @{"Authorization" = "Bearer $($clientResponse.accessToken)"}
        
        # Testar endpoint /me
        try {
            $meData = Invoke-RestMethod -Uri "http://localhost:5000/client-users/me" -Method GET -Headers $clientHeaders
            Write-Host "Endpoint /me OK: $($meData.name)" -ForegroundColor Green
            Write-Host "SISTEMA FUNCIONANDO CORRETAMENTE!" -ForegroundColor Green
        } catch {
            Write-Host "ERRO no endpoint /me: $($_.Exception.Message)" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "ERRO no login cliente: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Tentando criar usuario com senha..." -ForegroundColor Yellow
        
        # Criar usuario com senha
        $createUser = '{"name":"Cliente EmpresaTeste","email":"cliente@empresateste.com","password":"123456","role":"cliente","isActive":true}'
        try {
            $newUser = Invoke-RestMethod -Uri "http://localhost:5000/admin/users" -Method POST -Body $createUser -ContentType "application/json" -Headers $adminHeaders
            Write-Host "Usuario criado com senha via /admin/users" -ForegroundColor Green
            
            # Testar login novamente
            $clientResponse2 = Invoke-RestMethod -Uri "http://localhost:5000/auth/login" -Method POST -Body $clientLogin -ContentType "application/json"
            Write-Host "Login funcionou apos criar via /admin/users!" -ForegroundColor Green
        } catch {
            Write-Host "ERRO ao criar via /admin/users: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "Cliente NAO existe. Criando..." -ForegroundColor Yellow
    
    # Criar via /client-users
    $createClient = '{"name":"Cliente EmpresaTeste","email":"cliente@empresateste.com","document":"12345678901","phone":"(11) 99999-9999","address":"Rua Teste, 123"}'
    try {
        $newClient = Invoke-RestMethod -Uri "http://localhost:5000/client-users" -Method POST -Body $createClient -ContentType "application/json" -Headers $adminHeaders
        Write-Host "Cliente criado via /client-users (sem senha)" -ForegroundColor Green
    } catch {
        Write-Host "ERRO ao criar via /client-users: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    # Criar com senha via /admin/users
    $createUser = '{"name":"Cliente EmpresaTeste","email":"cliente@empresateste.com","password":"123456","role":"cliente","isActive":true}'
    try {
        $newUser = Invoke-RestMethod -Uri "http://localhost:5000/admin/users" -Method POST -Body $createUser -ContentType "application/json" -Headers $adminHeaders
        Write-Host "Usuario criado com senha via /admin/users" -ForegroundColor Green
    } catch {
        Write-Host "ERRO ao criar via /admin/users: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "DIAGNOSTICO CONCLUIDO" -ForegroundColor Green
