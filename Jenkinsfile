pipeline {
  agent any

  stages {

    stage('Restore') {
      steps {
        dir('ClassLibrary') {
          bat 'dotnet restore ClassLibrary.sln'
        }
      }
    }

    stage('Build') {
      steps {
        dir('ClassLibrary') {
          bat 'dotnet build ClassLibrary.sln -c Release --no-restore'
        }
      }
    }

    stage('Test') {
      steps {
        dir('ClassLibrary') {
          bat 'dotnet test ClassLibrary.sln -c Release --no-build'
        }
      }
    }
    stage('Publish (CD)') {
  steps {
    dir('ClassLibrary') {
      bat 'dotnet publish ClassLibrary.sln -c Release -o publish'
      bat 'dir publish'
    }
  }
}

  }
}
