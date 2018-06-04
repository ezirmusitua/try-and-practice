export class Logger {
  constructor(name = 'global') {
    this.name = name
  }

  info(...msgs) {
    console.log(this.name + ' - [INFO]', ...msgs)
  }
}

export const GlobalLogger = new Logger()