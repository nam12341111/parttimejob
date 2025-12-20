pipeline {
    agent none
    
    stages {
        stage('Checkout') {
            agent any
            steps {
                checkout scm
            }
        }
        
        stage('Build & Test') {
            agent {
                docker { 
                    image 'mcr.microsoft.com/dotnet/sdk:9.0' 
                    args '-u root:root'
                }
            }
            steps {
                sh 'dotnet restore'
                sh 'dotnet build --no-restore'
                sh 'dotnet test --no-build --verbosity normal'
            }
        }
        
        stage('Build Docker Image') {
            agent any 
            steps {
                script {
                    sh 'docker compose build api'
                }
            }
        }

        stage('Deploy') {
            agent any
            steps {
                script {
                    sh 'docker compose up -d api'
                }
            }
        }
    }
}
