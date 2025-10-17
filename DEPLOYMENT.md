# Deployment Guide for Railway

This guide will help you deploy the Markdown Collab application to Railway.

## Prerequisites

- A [Railway](https://railway.app) account
- This repository pushed to GitHub

## Step-by-Step Deployment

### 1. Create Railway Project

1. Log in to [Railway](https://railway.app)
2. Click **"New Project"**
3. Select **"Deploy from GitHub repo"**
4. Authorize Railway to access your GitHub account if needed
5. Select this repository

### 2. Add PostgreSQL Database

1. In your Railway project dashboard, click **"+ New"**
2. Select **"Database"**
3. Choose **"Add PostgreSQL"**
4. Railway will automatically:
   - Provision a PostgreSQL database
   - Create a `DATABASE_URL` environment variable
   - Link it to your application

### 3. Configure Environment Variables

In your Railway project, go to your application service and add these variables:

#### Required Variables

| Variable | Value | Description |
|----------|-------|-------------|
| `SitePassword` | `your-secure-password` | Password users need to access the site |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Enables production optimizations |

#### Optional Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_URLS` | Auto-configured | Port binding (Railway handles this) |
| `DATABASE_URL` | Auto-configured | PostgreSQL connection (auto-provided) |

### 4. Deploy

Railway will automatically:
1. Detect the .NET 9.0 application
2. Use `nixpacks.toml` for build configuration
3. Run `dotnet publish`
4. Start the application
5. Provide a public URL with automatic SSL

**Your application will be available at:**
```
https://your-project-name.up.railway.app
```

## Post-Deployment

### 1. Test the Application

1. Visit your Railway URL
2. You should be redirected to the login page
3. Enter the password you configured in `SitePassword`
4. Create a test diagram room
5. Open the same room in a different browser/incognito window to test real-time collaboration

### 2. Update the Site Password

**Important:** Change the default password immediately!

1. Go to your Railway project
2. Select your application service
3. Navigate to "Variables"
4. Update `SitePassword` to a secure value
5. Click "Deploy" to restart with the new password

### 3. Monitor Your Application

Railway provides:
- **Logs**: View application logs in real-time
- **Metrics**: Monitor CPU, memory, and network usage
- **Deployments**: View deployment history and rollback if needed

## Troubleshooting

### Application Won't Start

**Check the logs:**
1. Go to your Railway project
2. Click on your application service
3. Go to the "Deployments" tab
4. Click on the latest deployment
5. View the logs for errors

**Common issues:**
- Missing `SitePassword` environment variable
- Database connection issues (ensure PostgreSQL is linked)
- Port binding issues (ensure `ASPNETCORE_URLS` is not set or set correctly)

### Database Connection Errors

1. Verify PostgreSQL service is running in Railway
2. Check that `DATABASE_URL` environment variable exists
3. Ensure both services are in the same Railway project
4. Try redeploying the application

### Can't Access the Site

1. Check that the deployment succeeded
2. Verify the Railway URL is correct
3. Check browser console for errors
4. Try accessing in incognito mode

### Real-Time Updates Not Working

1. Verify WebSocket connections are allowed
2. Check browser console for SignalR errors
3. Ensure HTTPS is working (Railway provides this automatically)
4. Try refreshing both browser windows

## Environment-Specific Configuration

### Development
- Uses in-memory database if PostgreSQL not configured
- Default password: `changeme`
- Runs on `https://localhost:5001`

### Production (Railway)
- Uses PostgreSQL database
- Requires `SitePassword` environment variable
- Automatic HTTPS via Railway
- WebSocket support enabled

## Updating Your Application

### Method 1: Push to GitHub
1. Make changes to your code
2. Commit and push to your GitHub repository
3. Railway automatically detects changes and redeploys

### Method 2: Manual Deploy
1. Go to Railway project
2. Click on your application service
3. Go to "Deployments" tab
4. Click "Deploy" to trigger a new deployment

## Scaling Considerations

### For Higher Traffic:
1. **Upgrade Railway Plan**: Railway offers different pricing tiers
2. **Database Scaling**: Upgrade PostgreSQL instance in Railway
3. **Session Management**: Consider Redis for distributed sessions
4. **Multiple Instances**: Railway supports horizontal scaling

### Current Limitations:
- In-memory sessions (single instance only)
- No diagram expiration (consider adding TTL)
- No user authentication beyond site password

## Security Best Practices

1. ✅ Always use a strong `SitePassword`
2. ✅ Keep your Railway account secure with 2FA
3. ✅ Regularly update .NET dependencies
4. ✅ Monitor logs for suspicious activity
5. ✅ Consider adding rate limiting for production use

## Cost Estimation

Railway pricing is based on usage:
- **Hobby Plan**: $5/month (includes $5 credit)
- **Pro Plan**: $20/month (includes $20 credit)

Typical usage for this app:
- Small app: ~$5-10/month
- PostgreSQL: Included in plan credit
- SSL: Free (provided by Railway)

## Support

- **Railway Documentation**: https://docs.railway.app
- **Railway Discord**: https://discord.gg/railway
- **Application Issues**: Check the project's GitHub issues

## Quick Reference Commands

### Local Testing Before Deploy
```bash
# Build the project
dotnet build

# Run locally
dotnet run

# Test with custom password
export SitePassword="test123"
dotnet run
```

### Railway CLI (Optional)
```bash
# Install Railway CLI
npm i -g @railway/cli

# Login
railway login

# Link to project
railway link

# View logs
railway logs

# Add variable
railway variables set SitePassword=your-password
```

---

**Ready to deploy?** Follow the steps above and your collaborative markdown diagram editor will be live in minutes! 🚀
