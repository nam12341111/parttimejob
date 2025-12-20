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
                    // Thêm dotnet clean để xóa các file build cũ tránh lỗi access denied
                    bat 'docker run --rm -v "%WORKSPACE%":/app -w /app mcr.microsoft.com/dotnet/sdk:9.0 /bin/sh -c "dotnet clean && dotnet restore && dotnet build --no-restore && dotnet test --no-build --verbosity normal"'
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
