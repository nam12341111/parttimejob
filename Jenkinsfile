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
                    // Tắt container cũ để nhả port/resource (nếu có)
                    bat 'docker compose down || ver > nul'
                }
            }
        }
        
        stage('Build & Test') {
            steps {
                script {
                    // QUAN TRỌNG:
                    // Thay vì mount volume (-v) và build trực tiếp (gây lỗi Access Denied trên Windows),
                    // chúng ta sẽ COPY source code vào trong container (/app) và build tại đó.
                    // Cách này tách biệt hoàn toàn với file system của Windows.
                    bat 'docker run --rm -v "%WORKSPACE%":/source mcr.microsoft.com/dotnet/sdk:9.0 /bin/sh -c "mkdir -p /app && cp -a /source/. /app/ && cd /app && dotnet restore && dotnet build --no-restore && dotnet test --no-build --verbosity normal"'
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
