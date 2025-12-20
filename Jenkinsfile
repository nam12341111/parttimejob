pipeline {
    agent any
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
        stage('Build & Test') {
        
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