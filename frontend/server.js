import { createReadStream } from 'node:fs'
import { stat } from 'node:fs/promises'
import { createServer } from 'node:http'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

const distDir = path.join(__dirname, 'dist')
const apiTarget = process.env.API_TARGET || 'http://localhost:5002'
const port = Number(process.env.PORT || 5173)

const mimeTypes = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.svg': 'image/svg+xml',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.webp': 'image/webp',
  '.ico': 'image/x-icon'
}

function sendFile(res, filePath) {
  const ext = path.extname(filePath).toLowerCase()
  res.writeHead(200, {
    'Content-Type': mimeTypes[ext] || 'application/octet-stream'
  })
  createReadStream(filePath).pipe(res)
}

async function serveStatic(req, res) {
  const rawUrl = new URL(req.url || '/', `http://localhost:${port}`)
  const cleanPath = decodeURIComponent(rawUrl.pathname).replace(/^\/+/, '')
  const requestedPath = path.normalize(path.join(distDir, cleanPath || 'index.html'))

  if (!requestedPath.startsWith(distDir)) {
    res.writeHead(403)
    res.end('Forbidden')
    return
  }

  try {
    const info = await stat(requestedPath)
    if (info.isFile()) {
      sendFile(res, requestedPath)
      return
    }
  } catch {
    // SPA fallback below.
  }

  sendFile(res, path.join(distDir, 'index.html'))
}

function readBody(req) {
  return new Promise((resolve, reject) => {
    const chunks = []
    req.on('data', chunk => chunks.push(chunk))
    req.on('end', () => resolve(chunks.length ? Buffer.concat(chunks) : undefined))
    req.on('error', reject)
  })
}

async function proxyApi(req, res) {
  try {
    const body = ['GET', 'HEAD'].includes(req.method || 'GET') ? undefined : await readBody(req)
    const headers = { ...req.headers }
    delete headers.host
    delete headers.connection

    const upstream = await fetch(`${apiTarget}${req.url}`, {
      method: req.method,
      headers,
      body
    })

    res.writeHead(upstream.status, Object.fromEntries(upstream.headers.entries()))
    const buffer = Buffer.from(await upstream.arrayBuffer())
    res.end(buffer)
  } catch (error) {
    res.writeHead(502, { 'Content-Type': 'application/json; charset=utf-8' })
    res.end(JSON.stringify({
      message: 'Không kết nối được backend OrderApi.',
      detail: error.message
    }))
  }
}

const server = createServer(async (req, res) => {
  if ((req.url || '').startsWith('/api/')) {
    await proxyApi(req, res)
    return
  }

  await serveStatic(req, res)
})

server.listen(port, () => {
  console.log(`Frontend running at http://localhost:${port}`)
  console.log(`Proxying /api to ${apiTarget}`)
})
