pipeline {
    agent any

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Debug - list tree') {
  steps {
    bat 'echo === ROOT ==='
    bat 'dir'
    bat 'echo === TaigaProject ==='
    bat 'dir TaigaProject'
    bat 'echo === TaigaProject\\ClassLibrary ==='
    bat 'dir TaigaProject\\ClassLibrary'
    bat 'echo === Search SLN from root ==='
    bat 'dir /s /b *.sln'
  }
}


        stage('Debug - list files') {
              steps {
                dir('TaigaProject/ClassLibrary') {
                  bat 'dir'
    }
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

