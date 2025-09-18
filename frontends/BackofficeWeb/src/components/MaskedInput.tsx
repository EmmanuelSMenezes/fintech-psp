'use client';

import React, { forwardRef } from 'react';
import { formatDocument, formatPhone, unformatDocument, unformatPhone } from '@/utils/formatters';

interface MaskedInputProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'onChange'> {
  mask: 'document' | 'phone';
  onChange: (value: string) => void;
  value: string;
}

const MaskedInput = forwardRef<HTMLInputElement, MaskedInputProps>(
  ({ mask, onChange, value, ...props }, ref) => {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const inputValue = e.target.value;
      let formattedValue = inputValue;
      let unformattedValue = inputValue;

      switch (mask) {
        case 'document':
          formattedValue = formatDocument(inputValue);
          unformattedValue = unformatDocument(inputValue);
          break;
        case 'phone':
          formattedValue = formatPhone(inputValue);
          unformattedValue = unformatPhone(inputValue);
          break;
        default:
          break;
      }

      // Atualizar o valor do input com formatação
      e.target.value = formattedValue;
      
      // Chamar onChange com valor sem formatação
      onChange(unformattedValue);
    };

    const getFormattedValue = () => {
      switch (mask) {
        case 'document':
          return formatDocument(value);
        case 'phone':
          return formatPhone(value);
        default:
          return value;
      }
    };

    const getMaxLength = () => {
      switch (mask) {
        case 'document':
          return 18; // 00.000.000/0000-00
        case 'phone':
          return 15; // (00) 00000-0000
        default:
          return undefined;
      }
    };

    const getPlaceholder = () => {
      if (props.placeholder) return props.placeholder;
      
      switch (mask) {
        case 'document':
          return '000.000.000-00 ou 00.000.000/0000-00';
        case 'phone':
          return '(11) 99999-9999';
        default:
          return '';
      }
    };

    return (
      <input
        {...props}
        ref={ref}
        value={getFormattedValue()}
        onChange={handleChange}
        maxLength={getMaxLength()}
        placeholder={getPlaceholder()}
        className={`w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${props.className || ''}`}
      />
    );
  }
);

MaskedInput.displayName = 'MaskedInput';

export default MaskedInput;
