#!/bin/bash

# Configuração do SQS
echo "Criando fila SQS..."
awslocal sqs create-queue --queue-name video_content

# Configuração do S3
echo "Criando bucket S3..."
awslocal s3 mb s3://video-compactor


echo "Configuração do S3 e SQS concluída com sucesso!"