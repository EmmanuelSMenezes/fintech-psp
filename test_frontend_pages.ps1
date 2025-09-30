#!/usr/bin/env pwsh

Write-Host "=== TESTE PÁGINAS FRONTEND BACKOFFICE ===" -ForegroundColor Cyan

# Função para testar uma página
function Test-FrontendPage {
    param(
        [string]$Url,
        [string]$PageName
    )
    
    try {
        Write-Host "--- Testando $PageName ---" -ForegroundColor Yellow
        
        # Fazer requisição HTTP para a página
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 10
        
        if ($response.StatusCode -eq 200) {
            # Verificar se contém erro de aplicação
            if ($response.Content -match "Application error.*client-side exception") {
                Write-Host "❌ $PageName`: Erro de aplicação client-side detectado" -ForegroundColor Red
                return $false
            } elseif ($response.Content -match "<!DOCTYPE html>") {
                Write-Host "✅ $PageName`: Página carregou com sucesso" -ForegroundColor Green
                return $true
            } else {
                Write-Host "⚠️ $PageName`: Resposta inesperada" -ForegroundColor Yellow
                return $false
            }
        } else {
            Write-Host "❌ $PageName`: Status $($response.StatusCode)" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ $PageName`: Erro - $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Testar páginas específicas
$pages = @(
    @{ Url = "http://localhost:3000/"; Name = "Home" },
    @{ Url = "http://localhost:3000/transacoes"; Name = "Transações" },
    @{ Url = "http://localhost:3000/priorizacao"; Name = "Priorização" },
    @{ Url = "http://localhost:3000/usuarios"; Name = "Usuários" },
    @{ Url = "http://localhost:3000/contas"; Name = "Contas" },
    @{ Url = "http://localhost:3000/empresas"; Name = "Empresas" }
)

$successCount = 0
$totalCount = $pages.Count

foreach ($page in $pages) {
    if (Test-FrontendPage -Url $page.Url -PageName $page.Name) {
        $successCount++
    }
    Start-Sleep -Seconds 1
}

Write-Host ""
Write-Host "=== RESULTADO FINAL ===" -ForegroundColor Cyan
Write-Host "Páginas testadas: $totalCount" -ForegroundColor White
Write-Host "Páginas funcionando: $successCount" -ForegroundColor Green
Write-Host "Páginas com problema: $($totalCount - $successCount)" -ForegroundColor Red

if ($successCount -eq $totalCount) {
    Write-Host "🎉 Todas as páginas estão funcionando!" -ForegroundColor Green
} else {
    Write-Host "⚠️ Algumas páginas têm problemas" -ForegroundColor Yellow
}
