Write-Host "=== DEBUG DAS PÁGINAS PROBLEMÁTICAS ===" -ForegroundColor Yellow

function Debug-Page {
    param(
        [string]$Url,
        [string]$PageName
    )
    
    Write-Host "`n--- Debugando $PageName ---" -ForegroundColor White
    Write-Host "URL: $Url" -ForegroundColor Gray
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 15 -UseBasicParsing
        
        Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "Content Length: $($response.Content.Length)" -ForegroundColor Green
        
        if ($response.Content -match "Application error.*client-side exception") {
            Write-Host "❌ ERRO ENCONTRADO: Client-side exception detectado" -ForegroundColor Red
            return $false
        }
        elseif ($response.Content -match "_next.*error" -or $response.Content -match "500.*Internal Server Error") {
            Write-Host "❌ ERRO: Página de erro do Next.js detectada" -ForegroundColor Red
            return $false
        }
        elseif ($response.Content -match "<!DOCTYPE html>") {
            Write-Host "✅ Página HTML válida carregada" -ForegroundColor Green
            return $true
        }
        else {
            Write-Host "⚠️ Resposta inesperada" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "❌ ERRO na requisição: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

$transacoesOk = Debug-Page -Url "http://localhost:3000/transacoes" -PageName "Transações"
$priorizacaoOk = Debug-Page -Url "http://localhost:3000/priorizacao" -PageName "Priorização"
$homeOk = Debug-Page -Url "http://localhost:3000" -PageName "Home"

Write-Host "`n=== RESUMO ===" -ForegroundColor Yellow
Write-Host "Home: $(if($homeOk) { 'OK' } else { 'PROBLEMA' })"
Write-Host "Transações: $(if($transacoesOk) { 'OK' } else { 'PROBLEMA' })"
Write-Host "Priorização: $(if($priorizacaoOk) { 'OK' } else { 'PROBLEMA' })"
