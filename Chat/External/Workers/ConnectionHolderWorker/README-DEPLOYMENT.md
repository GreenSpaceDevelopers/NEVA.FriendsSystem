# ConnectionHolderWorker Deployment Guide

## Docker Build

Для сборки Docker образа выполните команду из корневой директории проекта:

```bash
# Сборка образа (из корневой директории Neva.friendsSystem)
docker buildx build --platform linux/amd64 -f Chat/External/Workers/ConnectionHolderWorker/Dockerfile -t cr.yandex/crp0ua90hrat22e231l5/connection-worker:latest . --push

# Или для локальной сборки без push
docker build -f Chat/External/Workers/ConnectionHolderWorker/Dockerfile -t connection-holder-worker:latest .
```

## K3s Deployment

### 1. Создание deployment.yaml

Создайте файл deployment.yaml:

```bash
cat << 'EOF' > deployment.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: workers
  labels:
    name: workers
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: connection-holder-worker
  namespace: workers
  labels:
    app: connection-holder-worker
    version: v1
spec:
  replicas: 2
  selector:
    matchLabels:
      app: connection-holder-worker
  template:
    metadata:
      labels:
        app: connection-holder-worker
        version: v1
    spec:
      containers:
      - name: connection-holder-worker
        image: cr.yandex/crp0ua90hrat22e231l5/connection-worker:latest
        imagePullPolicy: Always
        ports:
        - containerPort: 11001
          name: http
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: WebSocketHost
          value: "http://+:11001/"
        - name: RawMessageQueueConfig__Host
          value: "greenspacegg.ru"
        - name: RawMessageQueueConfig__Port
          value: "5672"
        - name: RawMessageQueueConfig__Username
          value: "Neva"
        - name: RawMessageQueueConfig__Password
          value: "QRr4JfRlDPd19IGWSA2Vrg=="
        - name: RawMessageQueueConfig__HMACKey
          valueFrom:
            secretKeyRef:
              name: chat-secrets
              key: hmac-key
        - name: IdentityClient__BaseUrl
          value: "https://identity.greenspacegg.ru"
        - name: Minio__Endpoint
          value: "greenspacegg.ru:9000"
        - name: Minio__AccessKey
          value: "minioadmin"
        - name: Minio__SecretKey
          value: "minioadmin"
        - name: Minio__BucketName
          value: "chat-files"
        - name: Minio__UseSSL
          value: "true"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        securityContext:
          allowPrivilegeEscalation: false
          runAsNonRoot: true
          runAsUser: 1000
          capabilities:
            drop:
            - ALL
      restartPolicy: Always
      securityContext:
        fsGroup: 1000
---
apiVersion: v1
kind: Service
metadata:
  name: connection-holder-worker-service
  namespace: workers
  labels:
    app: connection-holder-worker
spec:
  type: LoadBalancer
  ports:
  - port: 11001
    targetPort: 11001
    protocol: TCP
    name: http
  selector:
    app: connection-holder-worker
---
apiVersion: v1
kind: Secret
metadata:
  name: chat-secrets
  namespace: workers
type: Opaque
data:
  hmac-key: UFhjWnRxdkhDUHhDWHBmS2V4M2JITXR0NFNRbVVnczA=
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: connection-holder-worker-hpa
  namespace: workers
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: connection-holder-worker
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
EOF
```

### 2. Развертывание

```bash
# Применение манифеста
kubectl apply -f deployment.yaml

# Проверка статуса
kubectl get pods -n workers
kubectl get services -n workers
kubectl get hpa -n workers
```

### 3. Мониторинг

```bash
# Логи подов
kubectl logs -f deployment/connection-holder-worker -n workers

# Описание подов
kubectl describe pods -l app=connection-holder-worker -n workers

# Статус HPA
kubectl describe hpa connection-holder-worker-hpa -n workers

# Все ресурсы в неймспейсе
kubectl get all -n workers
```

## Конфигурация

### Ключевые переменные окружения

- `WebSocketHost: "http://+:11001/"` - **ВАЖНО**: используем wildcard (+) для привязки ко всем интерфейсам
- `RawMessageQueueConfig__Host` - RabbitMQ host
- `RawMessageQueueConfig__Port` - RabbitMQ port
- `RawMessageQueueConfig__Username` - RabbitMQ username
- `RawMessageQueueConfig__Password` - RabbitMQ password
- `RawMessageQueueConfig__HMACKey` - HMAC ключ для аутентификации
- `IdentityClient__BaseUrl` - URL сервиса идентификации
- `Minio__Endpoint` - Minio сервер для файлов
- `Minio__AccessKey` - Minio ключ доступа
- `Minio__SecretKey` - Minio секретный ключ
- `Minio__BucketName` - Minio бакет

### Ресурсы

- **Requests**: 256Mi RAM, 250m CPU
- **Limits**: 512Mi RAM, 500m CPU
- **Replicas**: 2-10 (автоматическое масштабирование)

### Сетевые настройки

- **Service Type**: LoadBalancer (для внешнего доступа)
- **Port**: 11001 (WebSocket)
- **URL для подключения**: `ws://your-domain.com:11001/ws/`

## Troubleshooting

### Проблемы с HttpListener

Если получаете ошибку "The request is not supported":
- Убедитесь, что `WebSocketHost` использует wildcard: `http://+:11001/`
- Не используйте `0.0.0.0` или `localhost` - только `+`

### Проблемы с Minio

Если получаете ошибку "endpoint is null or empty":
- Проверьте переменные `Minio__*` в deployment
- Убедитесь, что Minio сервер доступен

### Проблемы с подключением к RabbitMQ

```bash
# Проверка подключения к RabbitMQ
kubectl exec -it deployment/connection-holder-worker -n workers -- nc -zv greenspacegg.ru 5672
```

### Проблемы с внешним доступом

```bash
# Проверка сервиса
kubectl get services -n workers

# Проверка endpoints
kubectl get endpoints -n workers

# Проверка доступности порта
kubectl exec deployment/connection-holder-worker -n workers -- nc -zv localhost 11001
```

### Проблемы с памятью

```bash
# Мониторинг использования ресурсов
kubectl top pods -n workers
```

## Обновление

```bash
# Обновление образа
docker buildx build --platform linux/amd64 -t cr.yandex/crp0ua90hrat22e231l5/connection-worker:latest -f Chat/External/Workers/ConnectionHolderWorker/Dockerfile . --push

# Обновление deployment
kubectl set image deployment/connection-holder-worker connection-holder-worker=cr.yandex/crp0ua90hrat22e231l5/connection-worker:latest -n workers

# Или через rollout
kubectl rollout restart deployment/connection-holder-worker -n workers

# Проверка статуса обновления
kubectl rollout status deployment/connection-holder-worker -n workers
```

## Удаление

```bash
# Удалить все ресурсы
kubectl delete namespace workers

# Или удалить отдельные ресурсы
kubectl delete deployment connection-holder-worker -n workers
kubectl delete service connection-holder-worker-service -n workers
kubectl delete hpa connection-holder-worker-hpa -n workers
kubectl delete secret chat-secrets -n workers
```

## Важные замечания

1. **WebSocketHost**: Всегда используйте `http://+:11001/` для HttpListener
2. **Minio**: Обязательно настройте все переменные Minio
3. **LoadBalancer**: Используйте для внешнего доступа к WebSocket
4. **Права доступа**: Приложение запускается от непривилегированного пользователя
5. **Масштабирование**: HPA автоматически масштабирует по CPU и памяти 