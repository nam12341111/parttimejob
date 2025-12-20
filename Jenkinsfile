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
                    // Tắt container cũ
                    bat 'docker compose down || ver > nul'
                    // Dùng git clean để xóa triệt để file build cũ (bin/obj) ngay trên Windows
                    // -f: Force, -d: Directory, -x: Ignored files (bao gồm bin/obj)
                    bat 'git clean -fdx'
                }
            }
        }
        
        stage('Build & Test') {
            steps {
                script {
                    // Môi trường đã sạch, chỉ cần restore và build
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
