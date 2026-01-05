pipeline {
    agent any

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Load Credentials') {
            steps {
                script {
                    try {
                        withCredentials([
                            string(credentialsId: 'JWT_KEY', variable: 'JWT_KEY'),
                            string(credentialsId: 'AI__OpenAI__ApiKey', variable: 'AI_API_KEY')
                        ]) {
                            env.JWT__Key = env.JWT_KEY
                            env.AI__OpenAI__ApiKey = env.AI_API_KEY
                            echo "✓ Credentials loaded successfully"
                        }
                    } catch (Exception e) {
                        echo "⚠ Warning: Could not load credentials - ${e.message}"
                        env.JWT__Key = "dummy"
                        env.AI__OpenAI__ApiKey = "dummy"
                    }
                }
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
                    bat '''
                        echo JWT__Key=%JWT__Key%> .env
                        echo AI__OpenAI__ApiKey=%AI__OpenAI__ApiKey%>> .env
                        docker compose up -d api
                    '''
                }
            }
        }
    }
}
