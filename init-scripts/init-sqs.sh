#!/bin/bash

# Configuração do SQS
echo "Criando fila SQS..."
awslocal sqs create-queue --queue-name video-processing-queue

# Configuração do S3
echo "Criando bucket S3..."
awslocal s3 mb s3://videos

# Configuração de permissões do bucket (opcional)
awslocal s3api put-bucket-policy \
    --bucket videos \
    --policy '{
        "Version": "2012-10-17",
        "Statement": [
            {
                "Effect": "Allow",
                "Principal": "*",
                "Action": [
                    "s3:GetObject",
                    "s3:PutObject",
                    "s3:DeleteObject"
                ],
                "Resource": [
                    "arn:aws:s3:::videos",
                    "arn:aws:s3:::videos/*"
                ]
            }
        ]
    }'

echo "Configuração do S3 e SQS concluída com sucesso!"