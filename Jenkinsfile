pipeline {
    agent any
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
        stage('Build & Test') {
            steps {
                script {
                    // Dùng lệnh bat để gọi docker run, tự map thư mục hiện tại (%) vào /app
                    bat 'docker run --rm -v "%WORKSPACE%":/app -w /app mcr.microsoft.com/dotnet/sdk:9.0 dotnet restore'
                    bat 'docker run --rm -v "%WORKSPACE%":/app -w /app mcr.microsoft.com/dotnet/sdk:9.0 dotnet build --no-restore'
                    bat 'docker run --rm -v "%WORKSPACE%":/app -w /app mcr.microsoft.com/dotnet/sdk:9.0 dotnet test --no-build --verbosity normal'
                }
            }
        }
        
        stage('Build Docker Image') {
            steps {
                script {
                    bat 'docker compose build api'
                }
            }
        }

        stage('Deploy') {
            steps {
                script {
                    bat 'docker compose up -d api'
                }
            }
        }
    }
}