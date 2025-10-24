import { brotliCompressSync } from "zlib";

const brotliCompress = (buffer: Buffer) => brotliCompressSync(buffer);
export default brotliCompress;
