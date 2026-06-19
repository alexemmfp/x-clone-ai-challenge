import { test, expect } from '@playwright/test'

test('auth flow: register → post tweet → logout → login', async ({ page }) => {
  const unique = Date.now().toString()
  const email = `e2e${unique}@test.com`
  const password = 'Test1234!'

  // Register
  await page.goto('/register')
  await page.fill('input[type="text"]', `e2e${unique}`)
  await page.fill('input[type="email"]', email)
  await page.fill('input[type="password"]', password)
  await page.click('button[type="submit"]')
  await expect(page).toHaveURL('/')

  // Post a tweet
  await page.fill('textarea', 'Hello from Playwright E2E!')
  await page.click('text=Post')
  await expect(page.locator('text=Hello from Playwright E2E!')).toBeVisible()

  // Sign out
  await page.click('text=Sign out')
  await expect(page).toHaveURL('/login')

  // Log back in
  await page.fill('input[type="email"]', email)
  await page.fill('input[type="password"]', password)
  await page.click('button[type="submit"]')
  await expect(page).toHaveURL('/')
})
