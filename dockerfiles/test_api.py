#!/usr/bin/env python3
"""
Script para testar APIs do FintechPSP
Contorna problemas de SSL/TLS do PowerShell
"""

import requests
import json
import sys
from datetime import datetime

# Configurações
BASE_URL_AUTH = "http://localhost:5001"
BASE_URL_COMPANY = "http://localhost:5010"
BASE_URL_GATEWAY = "http://localhost:5000"

# Credenciais admin
ADMIN_EMAIL = "admin@fintechpsp.com"
ADMIN_PASSWORD = "admin123"

def print_header(title):
    print(f"\n{'='*60}")
    print(f"🧪 {title}")
    print(f"{'='*60}")

def print_success(message):
    print(f"✅ {message}")

def print_error(message):
    print(f"❌ {message}")

def print_info(message):
    print(f"ℹ️  {message}")

def get_admin_token():
    """Obter token JWT do admin"""
    print_header("STEP 1: AUTENTICAÇÃO ADMIN")
    
    url = f"{BASE_URL_AUTH}/auth/login"
    payload = {
        "email": ADMIN_EMAIL,
        "password": ADMIN_PASSWORD
    }
    
    try:
        response = requests.post(url, json=payload, timeout=10)
        response.raise_for_status()
        
        data = response.json()
        token = data.get('accessToken')
        
        print_success(f"Login realizado com sucesso")
        print_info(f"Token: {token[:50]}...")
        return token
        
    except requests.exceptions.RequestException as e:
        print_error(f"Erro no login: {e}")
        return None

def test_company_creation_direct(token):
    """Testar criação de empresa diretamente no CompanyService"""
    print_header("STEP 2: TESTE DIRETO - COMPANYSERVICE")
    
    url = f"{BASE_URL_COMPANY}/admin/companies"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    # CNPJ válido para teste
    payload = {
        "company": {
            "cnpj": "11.222.333/0001-81",
            "razaoSocial": "Empresa Teste Python LTDA",
            "nomeFantasia": "Teste Python Corp",
            "email": "contato@testepython.com",
            "telefone": "(11) 99999-8888",
            "endereco": "Rua Python, 123",
            "cidade": "São Paulo",
            "estado": "SP",
            "cep": "01234-567"
        }
    }
    
    try:
        print_info(f"POST {url}")
        print_info(f"Payload: {json.dumps(payload, indent=2)}")
        
        response = requests.post(url, json=payload, headers=headers, timeout=15)
        
        print_info(f"Status Code: {response.status_code}")
        print_info(f"Response Headers: {dict(response.headers)}")
        
        if response.status_code == 201:
            data = response.json()
            print_success("Empresa criada com sucesso!")
            print_info(f"ID: {data.get('id')}")
            print_info(f"Razão Social: {data.get('razaoSocial')}")
            print_info(f"CNPJ: {data.get('cnpj')}")
            return data.get('id')
        else:
            print_error(f"Erro na criação: {response.status_code}")
            print_error(f"Response: {response.text}")
            return None
            
    except requests.exceptions.RequestException as e:
        print_error(f"Erro na requisição: {e}")
        return None

def test_company_creation_gateway(token):
    """Testar criação de empresa via API Gateway"""
    print_header("STEP 3: TESTE VIA API GATEWAY")
    
    url = f"{BASE_URL_GATEWAY}/admin/companies"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    payload = {
        "company": {
            "cnpj": "22.333.444/0001-92",
            "razaoSocial": "Empresa Gateway Python LTDA",
            "nomeFantasia": "Gateway Python Corp",
            "email": "contato@gatewaypython.com",
            "telefone": "(11) 88888-7777",
            "endereco": "Rua Gateway, 456",
            "cidade": "São Paulo",
            "estado": "SP",
            "cep": "01234-890"
        }
    }
    
    try:
        print_info(f"POST {url}")
        print_info(f"Payload: {json.dumps(payload, indent=2)}")
        
        response = requests.post(url, json=payload, headers=headers, timeout=15)
        
        print_info(f"Status Code: {response.status_code}")
        
        if response.status_code == 201:
            data = response.json()
            print_success("Empresa criada via Gateway!")
            print_info(f"ID: {data.get('id')}")
            return data.get('id')
        else:
            print_error(f"Erro via Gateway: {response.status_code}")
            print_error(f"Response: {response.text}")
            return None
            
    except requests.exceptions.RequestException as e:
        print_error(f"Erro na requisição Gateway: {e}")
        return None

def test_api_key_creation(token, company_id):
    """Testar criação de API Key"""
    print_header("STEP 4: CRIAÇÃO DE API KEY")
    
    url = f"{BASE_URL_AUTH}/api-keys"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    payload = {
        "companyId": company_id,
        "name": "API Key Teste Python",
        "scopes": ["companies", "transactions", "balance"],
        "rateLimitPerMinute": 100
    }
    
    try:
        print_info(f"POST {url}")
        print_info(f"Payload: {json.dumps(payload, indent=2)}")
        
        response = requests.post(url, json=payload, headers=headers, timeout=15)
        
        print_info(f"Status Code: {response.status_code}")
        
        if response.status_code == 201:
            data = response.json()
            print_success("API Key criada com sucesso!")
            print_info(f"Public Key: {data.get('publicKey')}")
            print_info(f"Secret Key: {data.get('secretKey')}")
            return data.get('publicKey'), data.get('secretKey')
        else:
            print_error(f"Erro na criação de API Key: {response.status_code}")
            print_error(f"Response: {response.text}")
            return None, None
            
    except requests.exceptions.RequestException as e:
        print_error(f"Erro na requisição API Key: {e}")
        return None, None

def test_api_key_auth(public_key, secret_key):
    """Testar autenticação com API Key"""
    print_header("STEP 5: AUTENTICAÇÃO COM API KEY")
    
    url = f"{BASE_URL_AUTH}/api-keys/authenticate"
    payload = {
        "publicKey": public_key,
        "secretKey": secret_key
    }
    
    try:
        print_info(f"POST {url}")
        print_info(f"Payload: {json.dumps(payload, indent=2)}")
        
        response = requests.post(url, json=payload, timeout=10)
        
        print_info(f"Status Code: {response.status_code}")
        
        if response.status_code == 200:
            data = response.json()
            print_success("Autenticação API Key bem-sucedida!")
            print_info(f"Access Token: {data.get('accessToken')[:50]}...")
            print_info(f"Scopes: {data.get('scopes')}")
            return data.get('accessToken')
        else:
            print_error(f"Erro na autenticação API Key: {response.status_code}")
            print_error(f"Response: {response.text}")
            return None
            
    except requests.exceptions.RequestException as e:
        print_error(f"Erro na autenticação API Key: {e}")
        return None

def main():
    """Função principal"""
    print_header("TESTE COMPLETO - FLUXO 1 FINTECHPSP")
    print_info(f"Timestamp: {datetime.now().isoformat()}")
    
    # Step 1: Autenticação admin
    admin_token = get_admin_token()
    if not admin_token:
        print_error("Falha na autenticação. Abortando testes.")
        sys.exit(1)
    
    # Step 2: Teste direto CompanyService
    company_id = test_company_creation_direct(admin_token)
    
    # Step 3: Teste via Gateway
    gateway_company_id = test_company_creation_gateway(admin_token)
    
    # Step 4: Criação de API Key (usar company_id que funcionou)
    target_company_id = company_id or gateway_company_id
    if target_company_id:
        public_key, secret_key = test_api_key_creation(admin_token, target_company_id)
        
        # Step 5: Teste autenticação API Key
        if public_key and secret_key:
            api_token = test_api_key_auth(public_key, secret_key)
    
    print_header("RESUMO DOS TESTES")
    print_success("Testes concluídos!")
    print_info("Verifique os resultados acima para identificar problemas.")

if __name__ == "__main__":
    main()
