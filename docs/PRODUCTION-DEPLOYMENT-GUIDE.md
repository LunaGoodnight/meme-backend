# Production Deployment Guide

This guide will help you deploy the Meme Service API to production using Docker Compose.

## Prerequisites

- A VPS or server with Docker and Docker Compose installed
- Domain name (optional, but recommended for HTTPS)
- SSH access to your server

## Step 1: Prepare Your Server

### Install Docker and Docker Compose

```bash
# Update package list
sudo apt update

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add your user to docker group (optional, to run docker without sudo)
sudo usermod -aG docker $USER

# Install Docker Compose (if not already included)
sudo apt install docker-compose-plugin
```

### Verify Installation

```bash
docker --version
docker compose version
```

## Step 2: Clone Your Repository

```bash
# Clone your repository to the server
git clone <your-repository-url>
cd meme-backend/MemeService
```

## Step 3: Configure Environment Variables

### Create .env file

```bash
# Copy the example file
cp .env.example .env

# Edit the .env file
nano .env
```

### Generate Secure Passwords

```bash
# Generate MySQL root password
openssl rand -base64 32

# Generate MySQL application password
openssl rand -base64 32
```

### Fill in .env file

```bash
# DigitalOcean Spaces Configuration
AWS__AccessKey=your_actual_spaces_access_key
AWS__SecretKey=your_actual_spaces_secret_key
AWS__ServiceURL=https://sgp1.digitaloceanspaces.com

# Database Configuration
MYSQL_ROOT_PASSWORD=<paste_generated_root_password>
MYSQL_DATABASE=memes
MYSQL_USER=memeservice
MYSQL_PASSWORD=<paste_generated_app_password>
```

**Important**: Save these passwords securely! You'll need them if you ever need to access the database directly.

## Step 4: Configure Firewall

```bash
# Allow SSH (if using UFW)
sudo ufw allow OpenSSH

# Allow HTTP and HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Allow your API port (5001)
sudo ufw allow 5001/tcp

# Enable firewall
sudo ufw enable
```

## Step 5: Deploy the Application

### Build and Start Containers

```bash
# Build and start in detached mode
docker compose up -d --build

# View logs
docker compose logs -f

# Check running containers
docker compose ps
```

### Verify Deployment

```bash
# Check API health
curl http://localhost:5001/api/memes

# Or from your local machine
curl http://your-server-ip:5001/api/memes
```

## Step 6: Database Initialization

The database will be automatically created when containers start. To verify:

```bash
# Access MySQL container
docker exec -it meme-db mysql -u memeservice -p

# Enter your MYSQL_PASSWORD when prompted
# Then run:
SHOW DATABASES;
USE memes;
SHOW TABLES;
```

## Step 7: Set Up Reverse Proxy with HTTPS (Recommended)

### Option A: Using Nginx with Let's Encrypt

#### Install Nginx

```bash
sudo apt install nginx certbot python3-certbot-nginx
```

#### Configure Nginx

Create `/etc/nginx/sites-available/meme-api`:

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

#### Enable Site and Get SSL Certificate

```bash
# Enable the site
sudo ln -s /etc/nginx/sites-available/meme-api /etc/nginx/sites-enabled/

# Test configuration
sudo nginx -t

# Reload Nginx
sudo systemctl reload nginx

# Get SSL certificate
sudo certbot --nginx -d your-domain.com
```

### Option B: Using Caddy (Easier, Auto HTTPS)

#### Install Caddy

```bash
sudo apt install -y debian-keyring debian-archive-keyring apt-transport-https
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' | sudo tee /etc/apt/sources.list.d/caddy-stable.list
sudo apt update
sudo apt install caddy
```

#### Configure Caddy

Edit `/etc/caddy/Caddyfile`:

```
your-domain.com {
    reverse_proxy localhost:5001
}
```

#### Start Caddy

```bash
sudo systemctl reload caddy
```

Caddy will automatically obtain and renew SSL certificates!

## Step 8: Update Firewall for Reverse Proxy

If using reverse proxy, you can close direct access to port 5001:

```bash
# Remove direct API access
sudo ufw delete allow 5001/tcp

# Ensure HTTP/HTTPS are allowed
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
```

## Management Commands

### Start Services

```bash
docker compose up -d
```

### Stop Services

```bash
docker compose down
```

### Restart Services

```bash
docker compose restart
```

### View Logs

```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f api
docker compose logs -f db
```

### Update Application

```bash
# Pull latest code
git pull

# Rebuild and restart
docker compose up -d --build

# Remove old images (optional)
docker image prune -f
```

### Database Backup

```bash
# Create backup directory
mkdir -p ~/backups

# Backup database
docker exec meme-db mysqldump -u root -p$MYSQL_ROOT_PASSWORD memes > ~/backups/meme-db-$(date +%Y%m%d-%H%M%S).sql

# Restore from backup
docker exec -i meme-db mysql -u root -p$MYSQL_ROOT_PASSWORD memes < ~/backups/meme-db-20240101-120000.sql
```

### Automated Backups (Cron Job)

```bash
# Edit crontab
crontab -e

# Add daily backup at 2 AM
0 2 * * * cd ~/meme-backend/MemeService && docker exec meme-db mysqldump -u root -p$(grep MYSQL_ROOT_PASSWORD .env | cut -d '=' -f2) memes > ~/backups/meme-db-$(date +\%Y\%m\%d).sql

# Keep only last 7 days of backups
0 3 * * * find ~/backups/meme-db-*.sql -mtime +7 -delete
```

## Monitoring

### Check Container Health

```bash
docker compose ps
docker stats
```

### Check Disk Usage

```bash
# Docker disk usage
docker system df

# Volume size
docker volume ls
docker volume inspect memeservice_meme_db_data
```

### Clean Up Unused Resources

```bash
# Remove unused images
docker image prune -a

# Remove unused volumes (CAREFUL!)
docker volume prune

# Complete cleanup (VERY CAREFUL!)
docker system prune -a --volumes
```

## Troubleshooting

### Containers Won't Start

```bash
# Check logs
docker compose logs

# Check if ports are already in use
sudo netstat -tulpn | grep :5001
sudo netstat -tulpn | grep :3306
```

### Database Connection Issues

```bash
# Check database container is healthy
docker compose ps

# Test database connection
docker exec -it meme-db mysql -u memeservice -p

# Check connection string in .env matches compose.yaml
cat .env
```

### API Returns 502/503

```bash
# Check if API container is running
docker compose ps

# Check API logs
docker compose logs api

# Restart API
docker compose restart api
```

### Permission Issues

```bash
# Fix file permissions
sudo chown -R $USER:$USER ~/meme-backend

# Fix .env permissions (should not be world-readable)
chmod 600 .env
```

## Security Checklist

- [ ] .env file is not committed to git
- [ ] Strong passwords generated for database
- [ ] Firewall configured (UFW or iptables)
- [ ] HTTPS enabled via reverse proxy
- [ ] SSH key authentication enabled (password auth disabled)
- [ ] Regular backups scheduled
- [ ] Database port (3306) not exposed to public
- [ ] Keep Docker and system packages updated

## Performance Tuning

### Adjust Resource Limits

Edit `compose.yaml` if you need to adjust memory/CPU limits:

```yaml
deploy:
  resources:
    limits:
      cpus: '2.0'        # Increase if needed
      memory: 1G         # Increase if needed
```

### Monitor Resource Usage

```bash
# Real-time monitoring
docker stats

# Install monitoring tools
sudo apt install htop
htop
```

## Useful Links

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Nginx Documentation](https://nginx.org/en/docs/)
- [Caddy Documentation](https://caddyserver.com/docs/)
- [Let's Encrypt](https://letsencrypt.org/)

## Support

If you encounter issues:
1. Check the logs: `docker compose logs -f`
2. Verify environment variables: `cat .env`
3. Check container status: `docker compose ps`
4. Review this guide's troubleshooting section
