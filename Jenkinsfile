pipeline {
    agent any
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Clean Environment') {
            steps {
                script {
                    // Tắt các container cũ nếu đang chạy để nhả file (tránh lỗi Access denied)
                    bat 'docker compose down || ver > nul'
                }
            }
        }
        
        stage('Build & Test') {
            steps {
                script {
                    // Xóa file rác và Build lại
                    bat 'docker run --rm -v "%WORKSPACE%":/app -w /app mcr.microsoft.com/dotnet/sdk:9.0 /bin/sh -c "rm -rf */bin */obj && dotnet restore && dotnet build --no-restore && dotnet test --no-build --verbosity normal"'
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
