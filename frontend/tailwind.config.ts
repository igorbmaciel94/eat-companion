import type { Config } from 'tailwindcss'

export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        primary: { DEFAULT: '#016b61', dim: '#005e54', container: '#9ff2e4', fixed: '#9ff2e4', 'fixed-dim': '#91e3d6' },
        'on-primary': { DEFAULT: '#e2fff8', container: '#005c53', fixed: '#004840', 'fixed-variant': '#00675d' },
        secondary: { DEFAULT: '#4b645f', dim: '#3f5853', container: '#cce8e2', fixed: '#cce8e2', 'fixed-dim': '#bedad4' },
        'on-secondary': { DEFAULT: '#e3fff8', container: '#3d5752', fixed: '#2b4440', 'fixed-variant': '#47605c' },
        tertiary: { DEFAULT: '#336578', dim: '#26596b', container: '#b5e7fd', fixed: '#b5e7fd', 'fixed-dim': '#a7d9ee' },
        'on-tertiary': { DEFAULT: '#f2faff', container: '#225668', fixed: '#064355', 'fixed-variant': '#2d6072' },
        error: { DEFAULT: '#a83836', dim: '#67040d', container: '#fa746f' },
        'on-error': { DEFAULT: '#fff7f6', container: '#6e0a12' },
        surface: {
          DEFAULT: '#f6faf8',
          dim: '#d1dcd9',
          bright: '#f6faf8',
          variant: '#dae5e1',
          tint: '#016b61',
          container: { DEFAULT: '#e7f0ed', low: '#eef5f2', high: '#e1eae7', highest: '#dae5e1', lowest: '#ffffff' },
        },
        'on-surface': { DEFAULT: '#2a3432', variant: '#56615f' },
        outline: { DEFAULT: '#727d7a', variant: '#a9b4b1' },
        background: '#f6faf8',
        'on-background': '#2a3432',
        'inverse-surface': '#0a0f0e',
        'inverse-on-surface': '#999e9c',
        'inverse-primary': '#a7faec',
      },
      fontFamily: {
        headline: ['Manrope', 'sans-serif'],
        body: ['Inter', 'sans-serif'],
        label: ['Inter', 'sans-serif'],
      },
      borderRadius: {
        DEFAULT: '1rem',
        lg: '2rem',
        xl: '3rem',
        full: '9999px',
      },
    },
  },
  plugins: [require('@tailwindcss/forms')],
} satisfies Config
