Great choice! Let’s walk through setting up your **first custom subdomain site**:  
`meme.vividcats.org` — ready to run anything you want inside (let’s use a simple Hello World Node.js/Express app as the first demo, but you can swap it with any other stack later).

---

## **1. Make Sure DNS is Ready for Subdomain**

- **You already created a wildcard `*` A record** earlier, so `meme.vividcats.org` will work.
- If not, simply add an A record for `meme` pointing to `146.190.98.9`.
- Wait a few minutes for DNS propagation.

---

## **2. Create a Folder for the Project**

Let’s do all this on your server:

```bash
mkdir -p /srv/projects/meme-backend
cd /srv/projects/meme-backend
```

---

## **3. Create a Simple Hello World App (Node.js/Express Example)**

```bash
cat > index.js <<'EOF'
const express = require('express');
const app = express();
const port = 3001;
app.get('/', (req, res) => res.send('<h1>Hello from meme.vividcats.org</h1>'));
app.listen(port, () => console.log(`Server running on port ${port}`));
EOF
```

**Create a package.json:**
```bash
cat > package.json <<'EOF'
{
  "name": "mem-site",
  "version": "1.0.0",
  "main": "index.js",
  "dependencies": {
    "express": "^4.18.2"
  }
}
EOF
```

---

## **4. Create a Dockerfile**

```bash
cat > Dockerfile <<'EOF'
FROM node:18
WORKDIR /app
COPY package.json .
RUN npm install
COPY index.js .
CMD ["node", "index.js"]
EOF
```

---

## **5. Create docker-compose.yml**

```bash
cat > docker-compose.yml <<'EOF'
version: '3'
services:
  mem:
    build: .
    ports:
      - "3001:3001"
    restart: always
EOF
```

---

## **6. Build & Run your Project**

```bash
docker-compose up -d --build
```

---

## **7. Add Nginx Server Block for meme.vividcats.org**

Create a config file for this subdomain:

```bash
nano /etc/nginx/sites-available/api.meme.vividcats.org
```
Paste this in:

```nginx
server {
    listen 80;
    server_name api.meme.vividcats.org;

    location / {
        proxy_pass http://localhost:5001;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

**Enable the site and reload Nginx:**
```bash
ln -s /etc/nginx/sites-available/meme.vividcats.org /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx
```

---

## **8. Add an SSL Certificate for meme.vividcats.org**

```bash
certbot --nginx -d api.meme.vividcats.org
```
- Follow the prompts as before.

---

## **9. See if your API is crashing**

```bash
docker-compose logs api
```
- (do this in the folder where your backend’s docker-compose.yml is)


a different app, Next.js, or .NET, or if you hit any errors or want an even easier “copy-paste” script for new subdomains. You’re doing fantastic! 🚀**