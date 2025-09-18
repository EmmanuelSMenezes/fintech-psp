'use client';

import React from 'react';
import { Field, ErrorMessage, useField } from 'formik';
import MaskedInput from './MaskedInput';

interface FormFieldProps {
  name: string;
  label: string;
  type?: string;
  placeholder?: string;
  required?: boolean;
  mask?: 'document' | 'phone';
  as?: 'input' | 'select' | 'textarea';
  options?: Array<{ value: string; label: string }>;
  rows?: number;
  className?: string;
  helpText?: string;
  children?: React.ReactNode;
}

const FormField: React.FC<FormFieldProps> = ({
  name,
  label,
  type = 'text',
  placeholder,
  required = false,
  mask,
  as = 'input',
  options = [],
  rows = 3,
  className = '',
  helpText,
  children,
  ...props
}) => {
  const [field, meta, helpers] = useField(name);

  const hasError = meta.touched && meta.error;

  const renderInput = () => {
    if (mask) {
      return (
        <MaskedInput
          mask={mask}
          value={field.value || ''}
          onChange={(value) => helpers.setValue(value)}
          onBlur={() => helpers.setTouched(true)}
          placeholder={placeholder}
          className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
            hasError ? 'border-red-300' : 'border-gray-300'
          } ${className}`}
          {...props}
        />
      );
    }

    if (as === 'select') {
      return (
        <Field
          name={name}
          as="select"
          className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
            hasError ? 'border-red-300' : 'border-gray-300'
          } ${className}`}
          {...props}
        >
          {children || (
            <>
              <option value="">Selecione...</option>
              {options.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </>
          )}
        </Field>
      );
    }

    if (as === 'textarea') {
      return (
        <Field
          name={name}
          as="textarea"
          rows={rows}
          placeholder={placeholder}
          className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-vertical ${
            hasError ? 'border-red-300' : 'border-gray-300'
          } ${className}`}
          {...props}
        />
      );
    }

    return (
      <Field
        name={name}
        type={type}
        placeholder={placeholder}
        className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
          hasError ? 'border-red-300' : 'border-gray-300'
        } ${className}`}
        {...props}
      />
    );
  };

  return (
    <div className="space-y-1">
      <label className="block text-sm font-medium text-gray-700">
        {label}
        {required && <span className="text-red-500 ml-1">*</span>}
      </label>
      
      {renderInput()}
      
      {helpText && (
        <p className="text-xs text-gray-500">{helpText}</p>
      )}
      
      <ErrorMessage 
        name={name} 
        component="div" 
        className="text-red-500 text-sm" 
      />
    </div>
  );
};

export default FormField;
