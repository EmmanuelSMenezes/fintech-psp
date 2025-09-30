#!/usr/bin/env pwsh

Write-Host "=== TESTE ESPECÍFICO DAS PÁGINAS PROBLEMÁTICAS ===" -ForegroundColor Cyan

function Test-PageContent {
    param(
        [string]$Url,
        [string]$PageName
    )
    
    try {
        Write-Host "--- Testando $PageName ---" -ForegroundColor Yellow
        Write-Host "URL: $Url" -ForegroundColor Gray
        
        # Fazer requisição com mais detalhes
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 15 -UseBasicParsing
        
        Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor White
        Write-Host "Content Length: $($response.Content.Length)" -ForegroundColor White
        
        # Verificar conteúdo específico
        if ($response.Content -match "Application error.*client-side exception") {
            Write-Host "❌ ERRO ENCONTRADO: Client-side exception detectado" -ForegroundColor Red
            
            # Extrair mais detalhes do erro
            if ($response.Content -match "Application error: a client-side exception has occurred") {
                Write-Host "   Erro específico: Application error detectado" -ForegroundColor Red
            }
            return $false
        }
        elseif ($response.Content -match "<!DOCTYPE html>") {
            # Verificar se é uma página de erro do Next.js
            if ($response.Content -match "_next.*error" -or $response.Content -match "500.*Internal Server Error") {
                Write-Host "❌ ERRO: Página de erro do Next.js detectada" -ForegroundColor Red
                return $false
            }
            else {
                Write-Host "✅ Página HTML válida carregada" -ForegroundColor Green
                return $true
            }
        }
        else {
            Write-Host "⚠️ Resposta não é HTML válido" -ForegroundColor Yellow
            Write-Host "Primeiros 200 caracteres:" -ForegroundColor Gray
            Write-Host $response.Content.Substring(0, [Math]::Min(200, $response.Content.Length)) -ForegroundColor Gray
            return $false
        }
    }
    catch {
        Write-Host "❌ ERRO na requisição: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            Write-Host "   Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
        return $false
    }
}

# Testar as páginas específicas mencionadas pelo usuário
Write-Host "Testando as páginas que o usuário reportou como problemáticas..." -ForegroundColor Cyan

$problematicPages = @(
    @{ Url = "http://localhost:3000/transacoes"; Name = "Transações" },
    @{ Url = "http://localhost:3000/priorizacao"; Name = "Priorização" }
)

$results = @()

foreach ($page in $problematicPages) {
    $result = Test-PageContent -Url $page.Url -PageName $page.Name
    $results += @{ Name = $page.Name; Success = $result; Url = $page.Url }
    Write-Host ""
    Start-Sleep -Seconds 2
}

# Resumo final
Write-Host "=== RESUMO DOS TESTES ===" -ForegroundColor Cyan
foreach ($result in $results) {
    $status = if ($result.Success) { "✅ FUNCIONANDO" } else { "❌ COM PROBLEMA" }
    $color = if ($result.Success) { "Green" } else { "Red" }
    Write-Host "$($result.Name): $status" -ForegroundColor $color
}

$problemCount = ($results | Where-Object { -not $_.Success }).Count
if ($problemCount -gt 0) {
    Write-Host ""
    Write-Host "⚠️ $problemCount página(s) ainda com problema!" -ForegroundColor Red
    Write-Host "Recomendação: Verificar logs do container e console do navegador" -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "🎉 Todas as páginas estão funcionando!" -ForegroundColor Green
}
