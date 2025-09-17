#!/bin/bash

# Script para executar testes dos frontends React
# Bash script para Linux/macOS

echo "🧪 Executando testes dos frontends React..."

# Função para verificar se o comando foi executado com sucesso
test_command() {
    local exit_code=$1
    local test_name="$2"
    
    if [ $exit_code -eq 0 ]; then
        echo "✅ $test_name - PASSOU"
        return 0
    else
        echo "❌ $test_name - FALHOU"
        return 1
    fi
}

# Array para armazenar resultados dos testes
test_results=()

echo ""
echo "📋 Instalando dependências..."

# Instalar dependências do BackofficeWeb
echo ""
echo "🔧 Instalando dependências do BackofficeWeb..."
cd frontends/BackofficeWeb
npm install
test_command $? "BackofficeWeb - Instalação de dependências"
test_results+=($?)

# Executar testes do BackofficeWeb
echo ""
echo "🧪 Executando testes do BackofficeWeb..."
npm test -- --passWithNoTests --watchAll=false
test_command $? "BackofficeWeb - Testes unitários"
test_results+=($?)

# Voltar para o diretório raiz
cd ../..

# Instalar dependências do InternetBankingWeb
echo ""
echo "🔧 Instalando dependências do InternetBankingWeb..."
cd frontends/InternetBankingWeb
npm install
test_command $? "InternetBankingWeb - Instalação de dependências"
test_results+=($?)

# Executar testes do InternetBankingWeb
echo ""
echo "🧪 Executando testes do InternetBankingWeb..."
npm test -- --passWithNoTests --watchAll=false
test_command $? "InternetBankingWeb - Testes unitários"
test_results+=($?)

# Voltar para o diretório raiz
cd ../..

# Contar testes que passaram
passed_tests=0
total_tests=${#test_results[@]}

for result in "${test_results[@]}"; do
    if [ $result -eq 0 ]; then
        ((passed_tests++))
    fi
done

# Resumo dos resultados
echo ""
echo "📊 RESUMO DOS TESTES:"
echo "==================="

if [ $passed_tests -eq $total_tests ]; then
    echo "🎉 TODOS OS TESTES PASSARAM! ($passed_tests/$total_tests)"
    echo ""
    echo "✅ Frontends React estão funcionando corretamente!"
    exit 0
else
    failed_tests=$((total_tests - passed_tests))
    echo "⚠️  ALGUNS TESTES FALHARAM! ($passed_tests/$total_tests passaram, $failed_tests falharam)"
    echo ""
    echo "❌ Verifique os logs acima para mais detalhes."
    exit 1
fi

echo ""
echo "🏁 Script de testes concluído."
