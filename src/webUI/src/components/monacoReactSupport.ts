import type { OnMount } from '@monaco-editor/react'

type MonacoInstance = Parameters<OnMount>[1]

let reactSupportConfigured = false

export function configureMonacoLanguage(monaco: MonacoInstance, language: string) {
  if (language !== 'typescript' && language !== 'javascript') return

  const ts = monaco.languages.typescript
  const compilerOptions = {
    allowJs: true,
    allowNonTsExtensions: true,
    allowSyntheticDefaultImports: true,
    esModuleInterop: true,
    jsx: ts.JsxEmit.ReactJSX,
    jsxImportSource: 'react',
    module: ts.ModuleKind.ESNext,
    moduleResolution: ts.ModuleResolutionKind.NodeJs,
    noEmit: true,
    strict: true,
    target: ts.ScriptTarget.ES2020,
  }

  ts.typescriptDefaults.setCompilerOptions(compilerOptions)
  ts.javascriptDefaults.setCompilerOptions(compilerOptions)
  ts.typescriptDefaults.setDiagnosticsOptions({
    noSemanticValidation: false,
    noSyntaxValidation: false,
  })

  if (reactSupportConfigured) return

  ts.typescriptDefaults.addExtraLib(reactTypes, 'file:///node_modules/@types/react/index.d.ts')
  ts.typescriptDefaults.addExtraLib(reactJsxRuntimeTypes, 'file:///node_modules/@types/react/jsx-runtime.d.ts')
  ts.typescriptDefaults.addExtraLib(reactDomTypes, 'file:///node_modules/@types/react-dom/client.d.ts')
  reactSupportConfigured = true
}

export function reactModelPath(language: string) {
  return language === 'typescript' ? 'file:///workspace/answer.tsx' : undefined
}

const reactTypes = `
declare namespace React {
  type Key = string | number | bigint;
  type ReactText = string | number;
  type ReactNode = ReactElement | ReactText | boolean | null | undefined | ReactNode[];
  type SetStateAction<S> = S | ((previousState: S) => S);
  type Dispatch<A> = (value: A) => void;
  type DependencyList = readonly unknown[];

  interface Attributes {
    key?: Key | null | undefined;
  }

  interface RefAttributes<T> extends Attributes {
    ref?: ((instance: T | null) => void) | { current: T | null } | null;
  }

  interface ReactElement<P = unknown, T = string | JSXElementConstructor<P>> {
    type: T;
    props: P;
    key: Key | null;
  }

  interface SyntheticEvent<T = Element> {
    currentTarget: T;
    target: EventTarget & T;
    preventDefault(): void;
    stopPropagation(): void;
  }

  interface ChangeEvent<T = Element> extends SyntheticEvent<T> {}
  interface MouseEvent<T = Element> extends SyntheticEvent<T> {}
  interface KeyboardEvent<T = Element> extends SyntheticEvent<T> {}

  type EventHandler<E extends SyntheticEvent> = (event: E) => void;
  type ChangeEventHandler<T = Element> = EventHandler<ChangeEvent<T>>;
  type MouseEventHandler<T = Element> = EventHandler<MouseEvent<T>>;
  type KeyboardEventHandler<T = Element> = EventHandler<KeyboardEvent<T>>;

  type CSSProperties = Record<string, string | number | undefined>;
  type JSXElementConstructor<P> = (props: P) => ReactElement | null;
  type ComponentType<P = object> = JSXElementConstructor<P>;
  type FC<P = object> = FunctionComponent<P>;
  interface FunctionComponent<P = object> {
    (props: P): ReactElement | null;
  }

  interface DOMAttributes<T> {
    children?: ReactNode;
    onChange?: ChangeEventHandler<T>;
    onClick?: MouseEventHandler<T>;
    onKeyDown?: KeyboardEventHandler<T>;
    onSubmit?: EventHandler<SyntheticEvent<T>>;
  }

  interface HTMLAttributes<T> extends DOMAttributes<T> {
    className?: string;
    id?: string;
    role?: string;
    style?: CSSProperties;
    tabIndex?: number;
    title?: string;
    'aria-label'?: string;
    'aria-describedby'?: string;
    'aria-expanded'?: boolean;
    'aria-live'?: 'off' | 'polite' | 'assertive';
    'data-testid'?: string;
  }

  interface ButtonHTMLAttributes<T> extends HTMLAttributes<T> {
    disabled?: boolean;
    type?: 'button' | 'submit' | 'reset';
  }

  interface InputHTMLAttributes<T> extends HTMLAttributes<T> {
    checked?: boolean;
    disabled?: boolean;
    name?: string;
    placeholder?: string;
    type?: string;
    value?: string | number | readonly string[];
  }

  interface TextareaHTMLAttributes<T> extends HTMLAttributes<T> {
    disabled?: boolean;
    placeholder?: string;
    rows?: number;
    value?: string | readonly string[];
  }

  interface AnchorHTMLAttributes<T> extends HTMLAttributes<T> {
    href?: string;
    rel?: string;
    target?: string;
  }

  interface ImgHTMLAttributes<T> extends HTMLAttributes<T> {
    alt?: string;
    src?: string;
  }

  function createElement<P>(type: JSXElementConstructor<P> | string, props?: P | null, ...children: ReactNode[]): ReactElement<P>;
  function useCallback<T extends (...args: never[]) => unknown>(callback: T, deps: DependencyList): T;
  function useEffect(effect: () => void | (() => void), deps?: DependencyList): void;
  function useMemo<T>(factory: () => T, deps: DependencyList): T;
  function useRef<T>(initialValue: T): { current: T };
  function useState<S>(initialState: S | (() => S)): [S, Dispatch<SetStateAction<S>>];
}

declare module 'react' {
  export = React;
  export as namespace React;
}

declare global {
  namespace JSX {
    interface Element extends React.ReactElement {}
    interface ElementClass { render: unknown; }
    interface IntrinsicAttributes extends React.Attributes {}
    interface IntrinsicElements {
      a: React.AnchorHTMLAttributes<HTMLAnchorElement>;
      article: React.HTMLAttributes<HTMLElement>;
      aside: React.HTMLAttributes<HTMLElement>;
      button: React.ButtonHTMLAttributes<HTMLButtonElement>;
      div: React.HTMLAttributes<HTMLDivElement>;
      footer: React.HTMLAttributes<HTMLElement>;
      form: React.HTMLAttributes<HTMLFormElement>;
      h1: React.HTMLAttributes<HTMLHeadingElement>;
      h2: React.HTMLAttributes<HTMLHeadingElement>;
      h3: React.HTMLAttributes<HTMLHeadingElement>;
      header: React.HTMLAttributes<HTMLElement>;
      img: React.ImgHTMLAttributes<HTMLImageElement>;
      input: React.InputHTMLAttributes<HTMLInputElement>;
      label: React.HTMLAttributes<HTMLLabelElement>;
      li: React.HTMLAttributes<HTMLLIElement>;
      main: React.HTMLAttributes<HTMLElement>;
      nav: React.HTMLAttributes<HTMLElement>;
      p: React.HTMLAttributes<HTMLParagraphElement>;
      section: React.HTMLAttributes<HTMLElement>;
      span: React.HTMLAttributes<HTMLSpanElement>;
      strong: React.HTMLAttributes<HTMLElement>;
      textarea: React.TextareaHTMLAttributes<HTMLTextAreaElement>;
      ul: React.HTMLAttributes<HTMLUListElement>;
    }
  }
}
export {};
`

const reactJsxRuntimeTypes = `
import React = require('react');
export namespace JSX {
  export interface Element extends React.ReactElement {}
  export interface IntrinsicElements extends globalThis.JSX.IntrinsicElements {}
}
export function jsx(type: unknown, props: unknown, key?: React.Key): React.ReactElement;
export function jsxs(type: unknown, props: unknown, key?: React.Key): React.ReactElement;
export const Fragment: unique symbol;
`

const reactDomTypes = `
declare module 'react-dom/client' {
  import React = require('react');
  interface Root {
    render(children: React.ReactNode): void;
    unmount(): void;
  }
  export function createRoot(container: Element | DocumentFragment): Root;
}
`
