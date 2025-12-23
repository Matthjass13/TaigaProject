pipeline {
    agent any

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                dir('TaigaProject/ClassLibrary') {
                    bat 'dotnet restore ClassLibrary.sln'
                }
            }
        }

        stage('Build') {
            steps {
                dir('TaigaProject/ClassLibrary') {
                    bat 'dotnet build ClassLibrary.sln -c Release --no-restore'
                }
            }
        }

        stage('Test') {
            steps {
                dir('TaigaProject/ClassLibrary') {
                    bat 'dotnet test ClassLibrary.sln -c Release --no-build'
                }
            }
        }
    }
}

