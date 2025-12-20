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
                    // Chạy gộp các lệnh dotnet trong 1 container duy nhất để giữ lại các package đã restore (tránh lỗi NETSDK1064)
                    // Lưu ý: Dùng /bin/sh -c để chạy chuỗi lệnh trong Linux container
                    bat 'docker run --rm -v "%WORKSPACE%":/app -w /app mcr.microsoft.com/dotnet/sdk:9.0 /bin/sh -c "dotnet restore && dotnet build --no-restore && dotnet test --no-build --verbosity normal"'
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
